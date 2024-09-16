using System;
using System.Collections.Generic;
using Common.Playable.Move;
using Godot;

namespace Common.Playable;

public enum ResourceStatusEnum {
	Fatigue
}

public partial class Resource : Node
{
	[Export] public bool GodMode;
	[Export] public float Health = 100.0f;
	[Export] public float MaxHealth = 100.0f;
	[Export] public float Stamina = 100.0f;
	[Export] public float MaxStamina = 100.0f;
	[Export] public float StaminaRegenerationRate = 10.0f;
	[Export] public float FatigueThreshold = 20.4f;
	
	[Export] public bool PlayerModel;

	private readonly List<ResourceStatusEnum> _statuses = new();

	public void Update(double delta)
	{
		GainStamina(StaminaRegenerationRate * (float)delta);
	}

	public void GainStamina(float amount)
	{
		Stamina = Math.Min(amount, MaxStamina);
		if(Stamina > FatigueThreshold)
			_statuses.Remove(ResourceStatusEnum.Fatigue);
	}

	public void LossStamina(float amount)
	{
		if (GodMode) return;
		Stamina -= amount;
		if (Stamina < 1)
		{
			_statuses.Add(ResourceStatusEnum.Fatigue);
		}
	}

	public void GainHealth(float amount)
	{
		Health = Math.Min(amount, MaxHealth);
	}

	public void LossHealth(float amount)
	{
		if (GodMode) return;
		Health -= amount;
	}

	public void Pay(AMove move)
	{
		LossStamina(move.GetStaminaCost());
	}

	public bool CanBePaid(AMove move)
	{
		return Stamina - move.GetStaminaCost() >= Stamina;
	}
}
