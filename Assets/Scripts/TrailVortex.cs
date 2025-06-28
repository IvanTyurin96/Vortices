using Jundroo.SimplePlanes.ModTools.Parts.Attributes;
using System;

/// <summary>
/// A part modifier for SimplePlanes.
/// A part modifier is responsible for attaching a part modifier behaviour script to a game object within a part's hierarchy.
/// </summary>
[Serializable]
public class TrailVortex : Jundroo.SimplePlanes.ModTools.Parts.PartModifier
{
	[DesignerPropertyToggleButton(Label = "Visible in Designer")]
	public bool VisibleInDesigner = true;

	[DesignerPropertySlider(0.1f, 5f, 50, Label = "Size")]
	public float Size = 1f;

	[DesignerPropertySlider(0.1f, 10f, 100, Label = "Length")]
	public float Length = 1f;

	[DesignerPropertySlider(0.1f, 2f, 20, Label = "Speed")]
	public float Speed = 1f;

	[DesignerPropertySlider(0.1f, 5f, 50, Label = "Emission")]
	public float Emission = 1f;

	[DesignerPropertySlider(0.1f, 4f, 40, Label = "Random angle")]
	public float RandomAngleMultiplier = 1f;

	[DesignerPropertySlider(0.1f, 4f, 40, Label = "Random length")]
	public float RandomLengthMultiplier = 1f;

	[DesignerPropertySlider(3000f, 30000f, 10, Label = "Max particles count")]
	public int MaxParticles = 3000;

	[DesignerPropertySlider(0.1f, 1f, 10, Label = "Opacity")]
	public float Opacity = 1f;

	[DesignerPropertySlider(0f, 90f, 91, Label = "Grow start AoA")]
	public float GrowStartVisibilityAOA = 5f;

	[DesignerPropertySlider(0f, 90f, 91, Label = "Grow end AoA")]
	public float GrowEndVisibilityAOA = 10f;

	[DesignerPropertySlider(0f, 90f, 91, Label = "Fade start AoA")]
	public float FadeStartVisibilityAOA = 20f;

	[DesignerPropertySlider(0f, 90f, 91, Label = "Fade end AoA")]
	public float FadeEndVisibilityAOA = 45f;

	[DesignerPropertySlider(50f, 300f, 251, Label = "Min visibility speed, km/h")]
	public float MinVisibilitySpeed = 100f;

	[DesignerPropertySlider(50f, 300f, 251, Label = "Max visibility speed, km/h")]
	public float MaxVisibilitySpeed = 150f;

	[DesignerPropertyToggleButton(Label = "Visible for -AoA")]
	public bool VisibleForNegativeAngleOfAttack = true;

	/// <summary>
	/// Called when this part modifiers is being initialized as the part game object is being created.
	/// </summary>
	/// <param name="partRootObject">The root game object that has been created for the part.</param>
	/// <returns>The created part modifier behaviour, or <c>null</c> if it was not created.</returns>
	public override Jundroo.SimplePlanes.ModTools.Parts.PartModifierBehaviour Initialize(UnityEngine.GameObject partRootObject)
    {
        // Attach the behaviour to the part's root object.
        var behaviour = partRootObject.GetComponent<TrailVortexBehaviour>();
        return behaviour;
    }
}