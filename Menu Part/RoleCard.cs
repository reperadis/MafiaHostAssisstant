using Godot;

namespace MafiaHostAssistant;

public partial class RoleCard : Node
{
	[Export] private Label roleNameLabel;
	[Export] private Label roleDescriptionLabel;
	[Export] private Label countLabel;
	private int count;
	

	private RoleRecord roleInfo;
	
	public void SetRoleRecord(RoleRecord roleInfo)
	{
		this.roleInfo = roleInfo;
		roleNameLabel.Text = roleInfo.roleName;
		roleDescriptionLabel.Text = roleInfo.roleDescription;
	}

	public void AddRoleToList()
	{
		count++;
		RoleList.AddRole(roleInfo);
		countLabel.Text = count.ToString();
	}

	public void RemoveRoleFromList()
	{
		if (count == 0)
		{
			return;
		}

		count--;
		RoleList.RemoveRole(roleInfo);
		countLabel.Text = count.ToString();
	}

	public void ShowRoleFullInfo()
	{
		
	}
}
