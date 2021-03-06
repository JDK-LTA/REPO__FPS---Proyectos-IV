﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    public GameObject player;

    public delegate void OnInputContinous(float axis);
    public event OnInputContinous OnMouseX;
    public event OnInputContinous OnMouseY;
    public event OnInputContinous OnMoveForward;
    public event OnInputContinous OnMoveRight;

    public delegate void OnInputHold(bool hold);
    public event OnInputHold OnHoldShoot;
    public event OnInputHold OnHoldAim;
    public event OnInputHold OnHoldRun;

    public delegate void OnInputTrigger();
    public event OnInputTrigger OnTriggerJump;
    public event OnInputTrigger OnTriggerShoot;
    public event OnInputTrigger OnTriggerDash;
    public event OnInputTrigger OnTriggerDeliver;
    public event OnInputTrigger OnTriggerEscape;

    public delegate void OnInputDebug();
    public event OnInputDebug OnChangeWeapon;

    private bool movingForward = false, movingRight = false;

    private bool initiated = false;
    [HideInInspector] public bool lockInput = false;
    public void Init()
    {
        initiated = true;
    }
    public void ResetInit()
    {
        initiated = false;
    }

    public bool debug = false;

    // Update is called once per frame
    private void Update()
    {
        if (initiated)
        {
            #region Continous Input
            float aux = Input.GetAxis("Mouse X");
            OnMouseX?.Invoke(aux);
            aux = Input.GetAxis("Mouse Y");
            OnMouseY?.Invoke(aux);

            aux = Input.GetAxis("Vertical");
            if (aux != 0)
            {
                OnMoveForward?.Invoke(aux);
                movingForward = true;
            }
            else if (movingForward)
            {
                OnMoveForward?.Invoke(0);
                movingForward = false;
            }

            aux = Input.GetAxis("Horizontal");
            if (aux != 0)
            {
                OnMoveRight?.Invoke(aux);
                movingRight = true;
            }
            else if (movingRight)
            {
                OnMoveRight?.Invoke(0);
                movingRight = false;
            }
            #endregion

            #region Hold and Trigger Input

            //HOLDS
            if (!lockInput)
            {

                if (Input.GetButtonDown("Shoot"))
                {
                    OnHoldShoot?.Invoke(true);
                    OnTriggerShoot?.Invoke();
                }
                else if (Input.GetButtonUp("Shoot"))
                {
                    OnHoldShoot?.Invoke(false);
                }

                if (Input.GetButtonDown("Aim"))
                {
                    OnHoldAim?.Invoke(true);
                }
                else if (Input.GetButtonUp("Aim"))
                {
                    OnHoldAim?.Invoke(false);
                }

                if (Input.GetButtonDown("Run"))
                {
                    OnHoldRun?.Invoke(true);
                }
                else if (Input.GetButtonUp("Run"))
                {
                    OnHoldRun?.Invoke(false);
                }
                //

                //TRIGGERS
                if (Input.GetButtonDown("Jump"))
                {
                    OnTriggerJump?.Invoke();
                }
                if (Input.GetButtonDown("Dash"))
                {
                    OnTriggerDash?.Invoke();
                }
                if (Input.GetKeyDown(KeyCode.F))
                {
                    OnTriggerDeliver?.Invoke();
                }
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    OnTriggerEscape?.Invoke();
                }
                //
            }
            #endregion

            #region Debug Input
            if (debug && Input.GetKeyDown(KeyCode.R))
            {
                OnChangeWeapon();
            }

        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (debug)
            {
                GameManager.Instance.Init();
                Init();
                WeaponPrefabsLists.Instance.Init();
                WaveManager.Instance.Init();
                WeaponManager.Instance._player.Init();
            }
        }
        #endregion
    }
}
