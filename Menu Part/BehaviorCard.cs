using Godot;

namespace MafiaHostAssistant;

// TODO: Figure out deletion of those.
public partial class BehaviorCard : Control
{
	[Export] private Label behNameLabel;
	
	[Export] private Control tagsExpandButtonContent;
	[Export] private Control tagsContent;
	
	[Export] private Control configExpandButtonContent;
	[Export] private Control configContent;

	[Export] private Control createdSharedVarsExpandButtonContent;
	[Export] private Control createdSharedVarsContent;

	[Export] private Control readSharedVarsExpandButtonContent;
	[Export] private Control readSharedVarsContent;

	[Export] private PackedScene itemScene;
	
	[Export] private Texture2D tagTexture;

	private bool isTagsExpanded;
	private bool isConfigExpanded;
	private bool isCreatedSharedVarsExpanded;
	private bool isReadSharedVarsExpanded;
	
	public void SetUp(BehaviorRecord info, string name)
	{		
		behNameLabel.Text = name;

		foreach (string tag in info.tags)
		{
			Node item = itemScene.Instantiate();
			item.GetMeta("Label").As<Label>().Text = tag;
			//item.GetMeta("TextureRect").As<TextureRect>().Texture = tagTexture; // TODO: Add texture
			tagsContent.AddChild(item);
		}
		if (info.tags.Length == 0)
		{
			tagsExpandButtonContent.QueueFree();
			tagsContent.QueueFree();
		}

		foreach (VariableRecord record in info.ConfigurableVariables)
		{
			Node item = itemScene.Instantiate();
			item.GetMeta("Label").As<Label>().Text = record.varName;
			item.GetMeta("TextureRect").As<TextureRect>().Texture = record.varType.GetTypeTexture();
			configContent.AddChild(item);
		}
		if (info.ConfigurableVariables.Length == 0)
		{
			configExpandButtonContent.QueueFree();
			configContent.QueueFree();
		}

		foreach (VariableRecord record in info.AccessedSharedVariables)
		{
			Node item = itemScene.Instantiate();
			item.GetMeta("Label").As<Label>().Text = record.varName;
			item.GetMeta("TextureRect").As<TextureRect>().Texture = record.varType.GetTypeTexture();
			readSharedVarsContent.AddChild(item);
		}
		if (info.AccessedSharedVariables.Length == 0)
		{
			readSharedVarsExpandButtonContent.QueueFree();
			readSharedVarsContent.QueueFree();
		}
		
		foreach (VariableRecord record in info.CreatedSharedVariables)
		{
			Node item = itemScene.Instantiate();
			item.GetMeta("Label").As<Label>().Text = record.varName;
			item.GetMeta("TextureRect").As<TextureRect>().Texture = record.varType.GetTypeTexture();
			createdSharedVarsContent.AddChild(item);
		}
		if (info.CreatedSharedVariables.Length == 0)
		{
			createdSharedVarsExpandButtonContent.QueueFree();
			createdSharedVarsContent.QueueFree();
		}
	}
}
