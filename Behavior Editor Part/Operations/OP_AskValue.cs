using System.Collections.Generic;
using Godot;

namespace MafiaHostAssistant;

public sealed partial class OP_AskValue : OPMessagingOperation
{
	[Export] private Label varNameLabel;
	[Export] private TextureRect varTypeTextureRect;
	private BehaviorVariableHandler variableHandler;

	protected override void OnAddition(BehaviorEditor behaviorEditor)
	{
		base.OnAddition(behaviorEditor);
		variableHandler = new(behaviorEditor, this, Tr("TK:OP_FIELD_WRITE-TO"), BehaviorVariableType.Anything, varNameLabel, varTypeTextureRect);
	}

	public void OpenConfigWindow()
	{
		behaviorEditor.SetConfigWindowActive();
		variableHandler.CreateSelectionField(true);
		CreateMessageField();
	}

	public override OperationReference GetOperationReference()
	{
		return new OperationReference(OperationName.AskValue, new Arguments(variableHandler.Variable.TrueVariableName, variableHandler.Variable.VariableType, BEToShared(message)));
	}
	
	public override void Write(OperationReference.Arguments argumens)
	{
        Arguments args = (Arguments)argumens;
        variableHandler.RegisterVariable(ParentScope.FindVariableByName(args.assignToVarName));
		WriteMessage(args.message);
	}

	public override string GetReadableOpearationName()
	{
		return Tr("TK:OP_ASK-VALUE");
	}

	public override bool IsStateless => true;

	public class Arguments : OperationReference.Arguments
	{
		public string assignToVarName;
		public List<SharedDynamicStringElementData> message;
		public BehaviorVariableType varType;
		public Arguments(string assignToVarName, BehaviorVariableType varType, List<SharedDynamicStringElementData> message)
		{
			this.assignToVarName = assignToVarName;
			this.varType = varType;
			this.message = message;
		}
	}
}
