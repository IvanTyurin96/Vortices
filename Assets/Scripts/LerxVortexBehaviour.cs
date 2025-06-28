using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class LerxVortexBehaviour : Jundroo.SimplePlanes.ModTools.Parts.PartModifierBehaviour
{
	[SerializeField]
	private Transform Part;

	[SerializeField]
	private ParticleSystem ParticleSystem;

	[SerializeField]
	private GameObject Pyramid;

	private bool _visibleInDesigner;
	private float _size;
	private float _randomSize;
	private float _length;
	private float _speed;
	private float _emission;
	private float _randomAngleMultiplier;
	private float _angleOfAttackSensitivity;
	private float _angleOfSlipSensitivity;
	private float _maxAngleOfAttack;
	private float _maxAngleOfSlip;
	private int _maxParticles;
	private float _opacity;
	private float _growStartVisibilityAngleOfAttack;
	private float _growEndVisibilityAngleOfAttack;
	private float _fadeStartVisibilityAngleOfAttack;
	private float _fadeEndVisibilityAngleOfAttack;
	private float _minVisibilitySpeed;
	private float _maxVisibilitySpeed;
	private bool _visibleForNegativeAngleOfAttack;

	private const float Ms2Kmh = 3.6f;

	private void Start()
	{
		GetAndClampModifiers();
		SetPyramidVisibility();
		TuneVortex();
	}

	private void Update()
	{
		GetAndClampModifiers();
		TuneVortex();
		VortexControl();
	}

	private void GetAndClampModifiers()
	{
		var modifer = (LerxVortex)PartModifier;

		_visibleInDesigner = modifer.VisibleInDesigner;
		_size = Mathf.Clamp(modifer.Size, 0f, Mathf.Infinity);
		_randomSize = Mathf.Clamp(modifer.RandomSize, 0f, Mathf.Infinity);
		_length = Mathf.Clamp(modifer.Length, 0f, Mathf.Infinity);
		_speed = Mathf.Clamp(modifer.Speed, 0f, Mathf.Infinity);
		_emission = Mathf.Clamp(modifer.Emission, 0f, Mathf.Infinity);
		_randomAngleMultiplier = Mathf.Clamp(modifer.RandomAngleMultiplier, 0f, Mathf.Infinity);
		_angleOfAttackSensitivity = Mathf.Clamp(modifer.AngleOfAttackSensitivity, 0f, Mathf.Infinity);
		_angleOfSlipSensitivity = Mathf.Clamp(modifer.AngleOfSlipSensitivity, 0f, Mathf.Infinity);
		_maxAngleOfAttack = Mathf.Clamp(modifer.MaxAngleOfAttack, 0f, Mathf.Infinity);
		_maxAngleOfSlip = Mathf.Clamp(modifer.MaxAngleOfSlip, 0f, Mathf.Infinity);
		_maxParticles = Mathf.Clamp(modifer.MaxParticles, 0, System.Int32.MaxValue);
		_opacity = Mathf.Clamp(modifer.Opacity, 0f, 1f);
		_growStartVisibilityAngleOfAttack = Mathf.Clamp(modifer.GrowStartVisibilityAOA, 0f, 90f);
		_growEndVisibilityAngleOfAttack = Mathf.Clamp(modifer.GrowEndVisibilityAOA, 0f, 90f);
		_fadeStartVisibilityAngleOfAttack = Mathf.Clamp(modifer.FadeStartVisibilityAOA, 0f, 90f);
		_fadeEndVisibilityAngleOfAttack = Mathf.Clamp(modifer.FadeEndVisibilityAOA, 0f, 90f);
		_minVisibilitySpeed = Mathf.Clamp(modifer.MinVisibilitySpeed, 0f, Mathf.Infinity);
		_maxVisibilitySpeed = Mathf.Clamp(modifer.MaxVisibilitySpeed, 0f, Mathf.Infinity);
		_visibleForNegativeAngleOfAttack = modifer.VisibleForNegativeAngleOfAttack;
	}

	private void SetPyramidVisibility()
	{
		if (!ServiceProvider.Instance.GameState.IsInDesigner)
		{
			Pyramid.gameObject.SetActive(false);
		}
	}

	private void TuneVortex()
	{
		ParticleSystem.MainModule main = ParticleSystem.main;
		ParticleSystem.MinMaxCurve startSize = main.startSize;
		float defaultStartSize = 1.0f;
		float randomConstantMin = Mathf.Clamp(1f - _randomSize, 0f, 1f);
		float randomConstantMax = Mathf.Clamp(1f + _randomSize, 1f, Mathf.Infinity);
		startSize.constantMin = randomConstantMin * defaultStartSize * _size;
		startSize.constantMax = randomConstantMax * defaultStartSize * _size;
		main.startSize = startSize;
		main.maxParticles = _maxParticles;

		ParticleSystem.EmissionModule emission = ParticleSystem.emission;
		float defaultRateOverTime = 500f;
		emission.rateOverTime = defaultRateOverTime * _emission;

		if (_visibleInDesigner)
		{
			ParticleSystem.gameObject.SetActive(true);
		}
		else
		{
			ParticleSystem.gameObject.SetActive(false);
		}
	}

	private void VortexControl()
	{
		const float AngleLimit = 60f;

		float maximalAngleOfAttack = Mathf.Clamp(_maxAngleOfAttack, 0f, AngleLimit);
		float factAngleOfAttack = GetAngleOfAttack();
		float angleOfAttack = Mathf.Clamp(factAngleOfAttack * _angleOfAttackSensitivity, -maximalAngleOfAttack, maximalAngleOfAttack);
		float maximalAngleOfSlip = Mathf.Clamp(_maxAngleOfSlip, 0f, AngleLimit);
		float angleOfSlip = Mathf.Clamp(GetAngleOfSlip() * _angleOfSlipSensitivity, -maximalAngleOfSlip, maximalAngleOfSlip);
		ParticleSystem.transform.localEulerAngles = new Vector3(-angleOfAttack, 180f, 0f);

		ParticleSystem.MainModule main = ParticleSystem.main;
		float startSpeed = 40f * _speed;
		float startLifetime = 4f / Mathf.Clamp(startSpeed, 0.001f, Mathf.Infinity) * 2f * _length;
		float multiplier = startSpeed / Mathf.Clamp(startLifetime, 0.000001f, Mathf.Infinity);
		main.startLifetime = startLifetime;
		main.startSpeed = startSpeed;

		float speedVisibilityMultiplier = 1f;
		if (!ServiceProvider.Instance.GameState.IsInDesigner)
		{
			Rigidbody rigidbody = GetRigidbody();
			speedVisibilityMultiplier = Mathf.InverseLerp(_minVisibilitySpeed, _maxVisibilitySpeed, rigidbody.velocity.magnitude * Ms2Kmh);
		}
		float airDensityMultiplier = 1f;
		float visibilityMultiplier = airDensityMultiplier * speedVisibilityMultiplier * _opacity;
		if (!_visibleForNegativeAngleOfAttack && angleOfAttack < 0f)
		{
			visibilityMultiplier = 0f;
		}
		main.startColor = factAngleOfAttack >= 0f
			? new Color(1f, 1f, 1f, visibilityMultiplier * Mathf.Lerp(0f, 1f, Mathf.InverseLerp(_growStartVisibilityAngleOfAttack, _growEndVisibilityAngleOfAttack, factAngleOfAttack)))
			: new Color(1f, 1f, 1f, visibilityMultiplier * Mathf.Lerp(0f, 1f, Mathf.InverseLerp(-_growStartVisibilityAngleOfAttack, -_growEndVisibilityAngleOfAttack, factAngleOfAttack)));
		if (factAngleOfAttack > _fadeStartVisibilityAngleOfAttack || factAngleOfAttack < -_fadeStartVisibilityAngleOfAttack)
		{
			main.startColor = factAngleOfAttack >= 0f
				? new Color(1f, 1f, 1f, visibilityMultiplier * Mathf.Lerp(1f, 0f, Mathf.InverseLerp(_fadeStartVisibilityAngleOfAttack, _fadeEndVisibilityAngleOfAttack, factAngleOfAttack)))
				: new Color(1f, 1f, 1f, visibilityMultiplier * Mathf.Lerp(1f, 0f, Mathf.InverseLerp(-_fadeStartVisibilityAngleOfAttack, -_fadeEndVisibilityAngleOfAttack, factAngleOfAttack)));
		}

		ParticleSystem.ForceOverLifetimeModule forceOverLifetime = ParticleSystem.forceOverLifetime;
		float randomAngle = 5f * _randomAngleMultiplier;
		float randomAngleY = Random.Range(-randomAngle, randomAngle);
		float randomAngleX = Random.Range(-randomAngle, randomAngle);
		float forceY = -Mathf.Tan((angleOfAttack + randomAngleY) * Mathf.Deg2Rad) * multiplier;
		float forceX = Mathf.Tan((-angleOfSlip + randomAngleX) * Mathf.Deg2Rad) * multiplier;
		forceOverLifetime.y = new(forceY, forceY);
		forceOverLifetime.x = new(forceX, forceX);

		if (ServiceProvider.Instance.GameState.IsInDesigner)
		{
			main.startColor = new Color(1f, 1f, 1f, _opacity);
		}
	}

	private float GetAngleOfAttack()
	{
		if (!ServiceProvider.Instance.GameState.IsInDesigner)
		{
			Rigidbody rigidbody = GetRigidbody();

			Vector3 velocity = rigidbody.GetPointVelocity(this.transform.position);

			float forwardAngle = Vector3.Angle(this.transform.forward, velocity);
			float angleOfAttack = Vector3.Angle(this.transform.up, velocity) - 90f;
			if (forwardAngle > 90f)
			{
				angleOfAttack = 180f * Mathf.Sign(angleOfAttack) - angleOfAttack;
			}

			return angleOfAttack;
		}

		return 0f;
	}

	private float GetAngleOfSlip()
	{
		if (!ServiceProvider.Instance.GameState.IsInDesigner)
		{
			Rigidbody rigidbody = GetRigidbody();

			Vector3 velocity = rigidbody.GetPointVelocity(this.transform.position);

			float forwardAngle = Vector3.Angle(this.transform.forward, velocity);
			float angleOfSlip = Vector3.Angle(this.transform.right, velocity) - 90f;
			if (forwardAngle > 90f)
			{
				angleOfSlip = 180f * Mathf.Sign(angleOfSlip) - angleOfSlip;
			}

			return angleOfSlip;
		}

		return 0f;
	}

	private Rigidbody GetRigidbody()
	{
		return this.Part.transform.parent.parent.gameObject.GetComponent<Rigidbody>();
	}
}