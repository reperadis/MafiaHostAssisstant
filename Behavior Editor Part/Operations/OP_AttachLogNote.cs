using System.Collections.Generic;

namespace MafiaHostAssistant;

public sealed partial class OP_AttachLogNote : OPMessagingOperation
{
	public void OpenConfigWindow()
	{
		behaviorEditor.SetConfigWindowActive();
		CreateMessageField();
	}

	public override OperationReference GetOperationReference()
	{
		return new OperationReference(OperationName.AttachLogNote, new Arguments(BEToShared(message)));
	}

	public override void Write(OperationReference.Arguments argumens)
	{
		message = SharedToBE((argumens as Arguments).message);
	}

	public override string GetReadableOpearationName()
	{
		return Tr("TK:OP_ATTACH-LOG-NOTE");
	}

	public override bool IsStateless
	{
		get => true;
	}

	public class Arguments : OperationReference.Arguments
	{
		public List<SharedDynamicStringElementData> message;

		public Arguments(List<SharedDynamicStringElementData> message)
		{
			this.message = message;
		}
	}
}
