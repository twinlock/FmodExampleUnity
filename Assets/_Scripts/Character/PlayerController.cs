using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct GroundTagType {
    public string Tag;
    public int FMODParamValue;
    GroundTagType(string tag, int value) {
        Tag = tag;
        FMODParamValue = value;
    }
}

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private static readonly string GroundTypeParam = "GroundType";
    [SerializeField] private float _speed = 5.0f;
    [SerializeField] private float _footstepSpeed = 0.5f;
    [SerializeField] private float _minFootstepSpeed = 0.1f;
    [SerializeField] private float _characterHeight = 1.0f;
    [SerializeField] private CharacterController _controller;
    [SerializeField] private AudioEvent _footstepEvent;
    [SerializeField] private GroundTagType[] _groundTags;
    private readonly Dictionary<string, int> _groundTagDict = new Dictionary<string, int>();
    private Coroutine _stepRoutine;
    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _groundTagDict.Clear();
        foreach (var groundTag in _groundTags) {
            _groundTagDict.Add(groundTag.Tag, groundTag.FMODParamValue);
        }
    }
    #region FOR_THE_LOVE_OF_GOD_DONT_DO_THIS
    // PLEASE don't do this, this is just to demonstrate how to get foostep events.
    // these should very realisticically be from your animation system when the foot hits the ground.
    private void OnEnable() {
        _stepRoutine = StartCoroutine(StepRoutine());
    }
    private void OnDisable() {
        if (_stepRoutine != null) {
            StopCoroutine(_stepRoutine);
        }
    }
    private IEnumerator StepRoutine() {
        while (true) {
            OnFootstep();
            yield return new WaitForSeconds(_footstepSpeed);
        }
    }
    #endregion
    #region Footsteps
    // this is a VERY simple footstep event, you could add in additional parameters (like whether your running, walking, etc)
    private void OnFootstep() {
        if (_footstepEvent == null || _controller.velocity.magnitude < _minFootstepSpeed) {
            return;
        }
        // first figure out what the ground type is
        int groundType = GetGroundType();

        // then play the footstep event
        AudioManager.Get().Play(_footstepEvent, transform.position, gameObject, (GroundTypeParam, groundType));
    }
    private int GetGroundType() {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, _characterHeight)) {
            if (_groundTagDict.ContainsKey(hit.collider.tag)) {
                return _groundTagDict[hit.collider.tag];
            }
        }
        return 0;
    }
    #endregion
    private void FixedUpdate() {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(horizontalInput, 0, verticalInput);
        Vector3 velocity = direction * _speed;

        _controller.Move(velocity * Time.deltaTime);
    }
}
