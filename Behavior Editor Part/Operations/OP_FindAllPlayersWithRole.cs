using Godot;

namespace MafiaHostAssistant;
// TODO: If the string contained in the variable is empty, output all players, instead of none
public sealed partial class OP_FindAllPlayersWithRole : Operation
{
	[Export] private Label assignToVarNameLabel;
	[Export] private TextureRect assignToVarTypeTextureRect;
	[Export] private Label roleNameVarNameLabel;
	[Export] private TextureRect roleNameVarTypeTextureRect;

	private BehaviorEditor behaviorEditor;
	private BehaviorVariableHandler assignToVariableHandler;
	private BehaviorVariableHandler roleNameVariableHandler;	

	protected override void OnAddition(BehaviorEditor behaviorEditor)
	{
		this.behaviorEditor = behaviorEditor;
		assignToVariableHandler = new(behaviorEditor, this, Tr("TK:OP_FIELD_WRITE-TO"), BehaviorVariableType.ListOfPlayers, assignToVarNameLabel, assignToVarTypeTextureRect);
		roleNameVariableHandler = new(behaviorEditor, this, Tr("TK:OP_FIELD_ROLE-NAME"), BehaviorVariableType.String, roleNameVarNameLabel, roleNameVarTypeTextureRect);
	}

	public void OpenConfigWindow()
	{
		behaviorEditor.SetConfigWindowActive();
		assignToVariableHandler.CreateSelectionField(true);
		roleNameVariableHandler.CreateSelectionField(false);
	}

	public override OperationReference GetOperationReference()
	{
		return new OperationReference(OperationName.FindAllPlayersWithRole, new Arguments(assignToVariableHandler.Variable.TrueVariableName, roleNameVariableHandler.Variable.TrueVariableName));
	}

	public override bool IsStateless => true;

	public override string GetReadableOpearationName()
	{
		return Tr("TK:OP_FIND-ALL-PLAYERS-WITH-ROLE");
	}

	public override void Write(OperationReference.Arguments argumens)
	{
		Arguments args = (Arguments)argumens;
		assignToVariableHandler.RegisterVariable(ParentScope.FindVariableByName(args.writeToVarName));
		roleNameVariableHandler.RegisterVariable(ParentScope.FindVariableByName(args.writeToVarName));
	}

	public class Arguments : OperationReference.Arguments
	{
		public string writeToVarName;
		public string roleNameVarName;

		public Arguments(string writeToVarName, string roleNameVarName)
		{
			this.writeToVarName = writeToVarName;
			this.roleNameVarName = roleNameVarName;
		}
	}
}
