using Godot;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MafiaHostAssistant;

public interface IScopeOwner
{
	public void CreateSharedVariable(string variableName, VariableContainer defaultValue);
	public void WriteSharedVariable(string variableName, VariableContainer data);
	public VariableContainer ReadSharedVariable(string valueName);
	public string GetSpecifiedOwnerName();
	public void SetCompromised();
}

public class PersistentScope
{
    private Dictionary<string, VariableContainer> persistentVariables;
    private readonly List<string> accessedSharedVariables = new();
    private List<OperationReference> sequence;
    private CancellationToken exitCancellationToken;
    public IScopeOwner ScopeOwner { get; private set; }
    public string BehvaviourName { get; private set; }
    public string BehvaiourTypeSpecification { get; private set; }

    public async Task Initialize(BehaviorRecord behaviorInfo, Dictionary<string, object> config, string behaviorName, BehaviorType behaviorType, IScopeOwner scopeOwner, CancellationToken exitCancellationToken)
    {
        persistentVariables = new()
        {
            { "@True", new VariableContainer<bool>(true) },
            { "@False", new VariableContainer<bool>(false) }
        };
        if (config != null)
        {
			// Deep copy
			foreach (KeyValuePair<string, object> pair in JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(config, GSSC.GSS)))
            {
                persistentVariables.Add(pair.Key, VariableContainer.FromObject(pair.Value));
            }
        }
        this.exitCancellationToken = exitCancellationToken;

        ScopeOwner = scopeOwner;
        sequence = behaviorInfo.MainSequence;
        BehvaviourName = behaviorName;
        BehvaiourTypeSpecification = behaviorType switch
		{
			BehaviorType.ActiveActionWithPlayer => $"({TranslationServer.Translate("TK:ACTIVE-ACTION-WITH-PLAYER")})",
            BehaviorType.PassiveActionWithPlayer => $"({TranslationServer.Translate("TK:PASSIVE-ACTION-WITH-PLAYER")})",
            BehaviorType.PassiveActionWithUnion => $"({TranslationServer.Translate("TK:PASSIVE-ACTION-WITH-UNION")})",
            BehaviorType.WakingAlgorythm => $"({TranslationServer.Translate("TK:WAKING-ALGORYTHM")})",
			_ => throw new Exception("Unknow Behavior Type: " + behaviorType)
		};
        if (behaviorInfo.IniSequence != null)
        {
            await Execute(behaviorInfo.IniSequence, null, null);
        }
    }

    public async Task<bool> TryExecute(Dictionary<string, VariableContainer> inputParameters, WakeableHandlerWindow handler)
    {
        if (sequence == null)
        {
            return false;
        }
        foreach (string str in accessedSharedVariables)
        {
            inputParameters.Add(str, ScopeOwner.ReadSharedVariable(str));
        }
        return await Execute(sequence, inputParameters, handler);
    }

    private async Task<bool> Execute(List<OperationReference> sequence, Dictionary<string, VariableContainer> inputVariables, WakeableHandlerWindow handler)
    {
		ExecutionState state = new();
        ExecutionScope executionScope = new();
        await executionScope.Execute(
            inputVariables: inputVariables,
			sequence: sequence,
            parentExecutionScope: null,
            cancellationToken: exitCancellationToken,
            rootParent: this,
			handler: handler,
            state: state);
        return state.executionResult;
    }

    public void WriteRootVariable(string varName, VariableContainer value)
    {
        persistentVariables[varName] = value;
    }

    public void CreateRootVariable(string varName, VariableContainer value)
    {
        persistentVariables.Add(varName, value);
    }

    public VariableContainer ReadRootVariable(string varName)
    {
        if (persistentVariables.ContainsKey(varName))
        {
            return persistentVariables[varName];
        }
        return ScopeOwner.ReadSharedVariable(varName);
    }

    public void AddAccessedSharedVariable(string varName) // invoked in Ini; Added variables act as input parameters for future execution;
    {
        accessedSharedVariables.Add(varName);
    }

    public void CreateSharedVariable(string varName, VariableContainer defaultValue)
    {
        ScopeOwner.CreateSharedVariable(varName, defaultValue);
    }
}

public class ExecutionScope
{
    private ExecutionScope parentExecutionScope;
    private Dictionary<string, VariableContainer> variables;

    private ExecutionScope currentChildExecutionScope; // There will only be one at a time
    private CancellationToken cancellationToken;
    private PersistentScope rootParent;

	private ExecutionState state;
	private WakeableHandlerWindow handler;

    public async Task Execute(Dictionary<string, VariableContainer> inputVariables, List<OperationReference> sequence, ExecutionScope parentExecutionScope, PersistentScope rootParent, WakeableHandlerWindow handler, ExecutionState state, CancellationToken cancellationToken)
    {
        if (sequence == null)
        {
            GD.PushError("A null process was started!");
            return;
        }
        if (inputVariables != null)
        {
            variables = inputVariables;
        }
        else
        {
            variables = new();
        }
        this.cancellationToken = cancellationToken;
        this.parentExecutionScope = parentExecutionScope;
        this.rootParent = rootParent;
		this.handler = handler;
		this.state = state;

        foreach (OperationReference operation in sequence)
        {
            if (state.terminateProcessEcnountered)
            {
                break;
            }
            if (state.breakLoopDepth >= 1)
            {
                if (state.isLoop)
                {
                    state.breakLoopDepth--;
                }
                break;
            }
            if (state.breakScopeDepth >= 1)
            {
                state.breakScopeDepth--;
                break;
            }
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await ExecuteOperation(operation, cancellationToken);
            }
            catch (Exception)
            {
                // TODO: Clean up fields, handlers, etc
				rootParent.ScopeOwner.SetCompromised();
                throw;
            }
        }
    }

	public VariableContainer<T> GetVariable<T>(string variableName)
	{
		return (VariableContainer<T>)GetVariable(variableName);
	}

	public VariableContainer GetVariable(string variableName)
    {
        if (variables.ContainsKey(variableName))
        {
            return variables[variableName];
        }
        else
        {
            if (parentExecutionScope == null)
            {
                return rootParent.ReadRootVariable(variableName);
            }
            return parentExecutionScope.GetVariable(variableName);
        }
    }

    public void SetVariable(string variableName, VariableContainer value)
    {
        if (variables.ContainsKey(variableName))
        {
            variables[variableName] = value;
        }
        else
        {
            if (parentExecutionScope != null)
            {
                parentExecutionScope.SetVariable(variableName, value);
                return;
            }
			rootParent.WriteRootVariable(variableName, value);
        }
    }

    private async Task ExecuteOperation(OperationReference operation, CancellationToken cancellationToken)
    {
        switch (operation.OperationName) // TODO: Implement all operations, do something with cancellationToken
        {
            case OperationName.IfStatement:
                {
                    OP_IfStatement.Arguments arguments = (OP_IfStatement.Arguments)operation.Argumens;
                    await IfStatement(arguments.variableName, arguments.ifSequence, arguments.endIfs);
                }
                break;

            case OperationName.RequestRoute:
                break;

            case OperationName.CreatePersistentVariable:
                {
                    OPVarCreator.Arguments args = (OPVarCreator.Arguments)operation.Argumens;
                    rootParent.CreateRootVariable(args.varName, GetDefaultForType(args.varType));
                }
                break;

            case OperationName.CreateConfig:
                break;

            case OperationName.CreateSharedVariable:
                {
                    OPVarCreator.Arguments args = (OPVarCreator.Arguments)operation.Argumens;
                    rootParent.CreateSharedVariable(args.varName, GetDefaultForType(args.varType));
                }
                break;

            case OperationName.CreateVariable:
                variables.Add(((OPVarCreator.Arguments)operation.Argumens).varName, null);
                break;

            case OperationName.ReadSharedVariable:
                {
                    OPVarCreator.Arguments args = (OPVarCreator.Arguments)operation.Argumens;
                    rootParent.AddAccessedSharedVariable(args.varName);
                    parentExecutionScope.variables.Add(args.varName, rootParent.ScopeOwner.ReadSharedVariable(args.varName));
                }
                break;

            case OperationName.AskValue:
                {
                    OP_AskValue.Arguments args = (OP_AskValue.Arguments)operation.Argumens;
                    await AskValue(args.assignToVarName, args.varType, args.message);
                }
                break;


            case OperationName.AssignDirectly:
                {
                    OP_AssignVariableDirectly.Arguments arguments = (OP_AssignVariableDirectly.Arguments)operation.Argumens;
                    SetVariable(arguments.assingToVarName, VariableContainer.FromObject(arguments.assignedValue));
                }
                break;

            case OperationName.AttachLogNote:
                break;

            case OperationName.BinaryOperation:
                {
                    OP_LogicalOperation.Arguments args = (OP_LogicalOperation.Arguments)operation.Argumens;
                    BinaryOperation(args.leftVarName, args.rightVarName, args.operationType, args.assignToVarName);
                }
                break;

            case OperationName.CompareEquality:
                {
                    OP_CompareEquality.Arguments args = (OP_CompareEquality.Arguments)operation.Argumens;
                    VariableContainer left = GetVariable(args.leftVarName);
                    VariableContainer right = GetVariable(args.rightVarName);
                    SetVariable(args.assignToVarName, new VariableContainer<bool>(left.BoxedValue.Equals(right.BoxedValue)));
                }
                break;

            case OperationName.CompareIntegers:
                {
                    OP_CompareIntegers.Arguments args = (OP_CompareIntegers.Arguments)operation.Argumens;
                    CompareIntegers(args.leftVarName, args.rightVarName, args.operationType, args.assignToVarName);
                }
                break;

            case OperationName.ControlFlow:
                break;

            case OperationName.FindAllPlayersWithRole:
                break;

            case OperationName.ForeachLoop:
                break;

            case OperationName.GetValue:
                {
                    OP_GetValue.Arguments args = (OP_GetValue.Arguments)operation.Argumens;
                    GetValue(args.assignToVarName, args.getFromVarName, args.selectedOptionName);
                }
                break;

            case OperationName.HighlightPlayer:
                break;

            case OperationName.IntegerOperation:
                {
                    OP_IntegerOperation.Arguments args = (OP_IntegerOperation.Arguments)operation.Argumens;
                    IntegerOperation(args.leftVarName, args.rightVarName, args.operationType, args.assignToVarName);
                }
                break;

            case OperationName.LogicalOperation:
                break;

            case OperationName.Branch:
                break;

            default:
                throw new Exception($"Unknown Operation with name index {operation.OperationName}");
        }

        static VariableContainer GetDefaultForType(BehaviorVariableType type)
		{
			return type switch
            {
                BehaviorVariableType.Bool => new VariableContainer<bool>(false),
				BehaviorVariableType.Integer => new VariableContainer<int>(0),
				BehaviorVariableType.String => new VariableContainer<string>(string.Empty),
				BehaviorVariableType.Player => new VariableContainer<Player>(null),
				BehaviorVariableType.Union => new VariableContainer<ActionUnion>(null),
				BehaviorVariableType.ListOfBools => new VariableContainer<List<bool>>(new()),
				BehaviorVariableType.ListOfInts => new VariableContainer<List<int>>(new()),
				BehaviorVariableType.ListOfStrings => new VariableContainer<List<string>>(new()),
				BehaviorVariableType.ListOfPlayers => new VariableContainer<List<Player>>(new()),
				_ => throw new Exception("Trying to get default value for an unsupported variable type: " + type),
			};
		}

		async Task IfStatement(string variableName, List<OperationReference> ifSequence, List<OP_IfStatement.EndIfArguments> endIfs)
		{
			if (GetVariable<bool>(variableName).Value)
			{
				await StartNewScope(null, ifSequence, false);
			}

			foreach (OP_IfStatement.EndIfArguments args in endIfs)
			{
				if (args.state == 0) // Full End
				{
					break;
				}
				if (args.state == 1) // Else
				{
					await StartNewScope(null, args.sequence, false);
					break;
				}
				if (args.state == 1 && GetVariable<bool>(args.elseIfCaseVarName).Value) // Else If
				{
					await StartNewScope(null, args.sequence, false);
				}
			}
		}

		async Task AskValue(string assignToVarName, BehaviorVariableType varType, List<SharedDynamicStringElementData> message)
		{
			VariableContainer container = GetVariable(assignToVarName);
			object value = null;
			handler.DisplayDynamicString(SharedToDisplayable(message));
			switch (varType)
			{
				case BehaviorVariableType.Bool:
					{
						// TODO: A better hint can be displayed in the label
						handler.CreateBoolField(assignToVarName, container.As<bool>().Value, b => value = b);
					}
					break;
				
				case BehaviorVariableType.Integer:
					{
						handler.CreateIntField(assignToVarName, container.As<int>().Value, true, b => value = b);
					}
					break;
				
				case BehaviorVariableType.String:
					{
						handler.CreateStringField(assignToVarName, container.As<string>().Value, b => value = b);
					}
					break;
					// TODO: Continue with the rest of the types
			}
			await handler.AwaitConfirmation();
			SetVariable(assignToVarName, VariableContainer.FromObject(value));
		}

		void BinaryOperation(string leftVarName, string rightVarName, OP_LogicalOperation.OperationType operationType, string assignToVarName)
		{
			VariableContainer<bool> left = GetVariable<bool>(leftVarName);
			VariableContainer<bool> right = GetVariable<bool>(rightVarName);
            var result = operationType switch
            {
                OP_LogicalOperation.OperationType.Conjunction => left.Value && right.Value,
                OP_LogicalOperation.OperationType.Disjunction => left.Value || right.Value,
                OP_LogicalOperation.OperationType.ExclusiveDisjunction => left.Value ^ right.Value,
                OP_LogicalOperation.OperationType.Implication => !left.Value || right.Value,
                OP_LogicalOperation.OperationType.ConverseImplication => left.Value || !right.Value,
                OP_LogicalOperation.OperationType.Equivalence => left.Value == right.Value,
                OP_LogicalOperation.OperationType.NonConjunction => !(left.Value && right.Value),
                OP_LogicalOperation.OperationType.NonDisjunction => !(left.Value || right.Value),
                OP_LogicalOperation.OperationType.NonImplication => left.Value && !right.Value,
                OP_LogicalOperation.OperationType.ConverseNonImplication => !left.Value && right.Value,
                _ => throw new Exception("Unknown Binary Operation Type: " + operationType),
            };
			SetVariable(assignToVarName, new VariableContainer<bool>(result));
        }

		void CompareIntegers(string leftVarName, string rightVarName, OP_CompareIntegers.OperationType operationType, string assignToVarName)
		{
			VariableContainer<int> left = GetVariable<int>(leftVarName);
			VariableContainer<int> right = GetVariable<int>(rightVarName);
			var result = operationType switch
			{
				OP_CompareIntegers.OperationType.GreaterThan => left.Value > right.Value,
				OP_CompareIntegers.OperationType.LessThan => left.Value < right.Value,
				OP_CompareIntegers.OperationType.Equal => left.Value == right.Value,
				OP_CompareIntegers.OperationType.NotEqual => left.Value != right.Value,
				OP_CompareIntegers.OperationType.GreaterThanOrEqual => left.Value >= right.Value,
				OP_CompareIntegers.OperationType.LessThanOrEqual => left.Value <= right.Value,
				_ => throw new Exception("Unknown Integer Comparison Operation Type: " + operationType),
			};
			SetVariable(assignToVarName, new VariableContainer<bool>(result));
		}

		void IntegerOperation(string leftVarName, string rightVarName, OP_IntegerOperation.OperationType operationType, string assignToVarName)
		{
			VariableContainer<int> left = GetVariable<int>(leftVarName);
			VariableContainer<int> right = GetVariable<int>(rightVarName);
			var result = operationType switch
			{
				OP_IntegerOperation.OperationType.Addition => left.Value + right.Value,
				OP_IntegerOperation.OperationType.Subtraction => left.Value - right.Value,
				OP_IntegerOperation.OperationType.Multiplication => left.Value * right.Value,
				OP_IntegerOperation.OperationType.IntegerDivision => left.Value / right.Value,
				OP_IntegerOperation.OperationType.Modulo => left.Value % right.Value,
				_ => throw new Exception("Unknown Integer Operation Type: " + operationType),
			};
			SetVariable(assignToVarName, new VariableContainer<int>(result));
		}

		void GetValue(string assignToVarName, string getFromVarName, OP_GetValue.OptionName option)
		{
            VariableContainer getFrom = GetVariable(getFromVarName);
			switch (option)
			{
				case OP_GetValue.OptionName.RoleName:
					SetVariable(assignToVarName, new VariableContainer<string>(getFrom.As<Player>().Value.roleInfo.roleName));
					break;

				case OP_GetValue.OptionName.IsAlive:
					SetVariable(assignToVarName, new VariableContainer<bool>(getFrom.As<Player>().Value.IsAlive));
					break;

				case OP_GetValue.OptionName.Players:
					// TODO: Don't fogwt to make copies of all reference-type objects manipulated as variables
					SetVariable(assignToVarName, new VariableContainer<List<Player>>(new List<Player>(getFrom.As<ActionUnion>().Value.CachedPlayersList)));
					break;

				case OP_GetValue.OptionName.InverseValue:
					SetVariable(assignToVarName, new VariableContainer<bool>(!getFrom.As<bool>().Value));
					break;

				case OP_GetValue.OptionName.Incremented:
					SetVariable(assignToVarName, new VariableContainer<int>(getFrom.As<int>().Value + 1));
					break;

				case OP_GetValue.OptionName.Decremented:
					SetVariable(assignToVarName, new VariableContainer<int>(getFrom.As<int>().Value - 1));
					break;
				
				case OP_GetValue.OptionName.Absolute:
					SetVariable(assignToVarName, new VariableContainer<int>(Math.Abs(getFrom.As<int>().Value)));
					break;

				default:
					throw new Exception($"Unknown option name in GetValue: {option}");
			}
		}

		List<DisplayableDynamicStringElementData> SharedToDisplayable(List<SharedDynamicStringElementData> shared)
		{
			List<DisplayableDynamicStringElementData> displayable = new();

			foreach (SharedDynamicStringElementData element in shared)
			{
				if (element.isVariable && element.variableType.IsList())
				{
                    if (GetVariable(element.stringData).BoxedValue is IList list)
                    {
                        foreach (object item in list)
                        {
                            displayable.Add(new DisplayableDynamicStringElementData(
                                isVariable: true,
                                variableType: element.variableType,
                                stringSource: new DirectStringSource(ValueToString(item))
                            ));
                        }
                        continue;
                    }
                }

				displayable.Add(new DisplayableDynamicStringElementData(
					isVariable: element.isVariable,
					variableType: element.variableType,
					stringSource: GetSource(element)
				));
			}
			
			return displayable;

			IStringSource GetSource(SharedDynamicStringElementData element)
			{
				IStringSource source = null;

				if (element.isVariable)
				{
					if (element.variableType == BehaviorVariableType.Player)
					{
						source = GetVariable<Player>(element.stringData).Value;
					}
					else if (element.variableType == BehaviorVariableType.Union)
					{
						source = GetVariable<ActionUnion>(element.stringData).Value;
					}
					else
					{
						source = new DirectStringSource(ValueToString(GetVariable(element.stringData).BoxedValue));
					}
				}
				else
				{
					source = new DirectStringSource(element.stringData);
				}
				return source;
			}
		}

		string ValueToString(object obj)
		{
			return obj switch
			{
				bool b => b ? TranslationServer.Translate("TK:TRUE") : TranslationServer.Translate("TK:FALSE"),
				int i => i.ToString(),
				string s => s,
				Player p => p.Name,
				ActionUnion u => u.UnionName,
				List<bool> lb => string.Join(", ", lb),
				List<int> li => string.Join(", ", li),
				List<string> ls => string.Join(", ", ls),
				List<Player> lp => string.Join(", ", lp.ConvertAll(p => p.Name)),
				_ => throw new Exception($"Unknown Object Type: {obj.GetType()}")
			};
		}
    }

    private async Task StartNewScope(Dictionary<string, VariableContainer> grantedVars, List<OperationReference> actions, bool isLoop)
    {
        if (actions.Count == 0)
        {
            return;
        }
        currentChildExecutionScope = new ExecutionScope();

        await currentChildExecutionScope.Execute(grantedVars, actions, this, rootParent, handler, state, cancellationToken);
    }
}

public abstract class VariableContainer
{
	public abstract object BoxedValue { get; }

    public static VariableContainer FromObject(object obj)
    {
        return obj switch
        {
            bool b => new VariableContainer<bool>(b),
            int i => new VariableContainer<int>(i),
            string s => new VariableContainer<string>(s),
            Player p => new VariableContainer<Player>(p),
            ActionUnion u => new VariableContainer<ActionUnion>(u),
            List<bool> lb => new VariableContainer<List<bool>>(lb),
            List<int> li => new VariableContainer<List<int>>(li),
            List<string> ls => new VariableContainer<List<string>>(ls),
            List<Player> lp => new VariableContainer<List<Player>>(lp),
            _ => throw new Exception($"Object type is undefined! (VariableContainer.FromObject) (type: {obj.GetType()})"),
        };
    }

	public VariableContainer<T> As<T>()
	{
		return (VariableContainer<T>)this;
	}
}
// If an object is null, its type is not preserved; VariableContainers are never null, 
// however their contents may be null; in this case the type is saved by the generic argument
public class VariableContainer<T> : VariableContainer
{
    public T Value;

	public override object BoxedValue => Value;

    public VariableContainer(T value)
    {
        Value = value;
    }
}

public class ExecutionState
{
    public bool terminateProcessEcnountered;
    public int breakScopeDepth;
    public int breakLoopDepth;
    public bool executionResult;
	public bool isLoop;
}