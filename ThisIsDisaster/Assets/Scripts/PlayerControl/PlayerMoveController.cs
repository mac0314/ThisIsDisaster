﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimatorUtil {
    public static bool HasParameter(Animator animator, string paramName) {
        foreach (var v in animator.parameters) {
            if (v.name == paramName) {
                return true;
            }
        }
        return false;
    }

    public static void SetInteger(Animator animator, string paramName, int value) {
        if (animator == null) return;
        if (HasParameter(animator, paramName))
        {
            animator.SetInteger(paramName, value);
        }
        else {
#if UNITY_EDITOR
            string log = string.Format("Animator of [{0}] Doesn't has Parameter [{1}]", animator.gameObject.name, paramName);
            Debug.LogError(log);
#endif
        }
    }

    public static void SetBool(Animator animator, string paramName, bool value)
    {
        if (animator == null) return;
        if (HasParameter(animator, paramName))
        {
            animator.SetBool(paramName, value);
        }
        else
        {
#if UNITY_EDITOR
            string log = string.Format("Animator of [{0}] Doesn't has Parameter [{1}]", animator.gameObject.name, paramName);
            Debug.LogError(log);
#endif
        }
    }

    public static void SetTrigger(Animator animator, string paramName)
    {
        if (animator == null) return;
        if (HasParameter(animator, paramName))
        {
            animator.SetTrigger(paramName);
        }
        else
        {
#if UNITY_EDITOR
            string log = string.Format("Animator of [{0}] Doesn't has Parameter [{1}]", animator.gameObject.name, paramName);
            Debug.LogError(log);
#endif
        }
    }
}

[System.Serializable]
public class Timer
{
    public delegate void OnTimerRunningEnd();

    public float elapsed = 0f;
    public float maxTime = 0f;
    public bool started = false;
    public bool autoStop = true;
    public int jumping = 0;
    public float Rate
    {
        get
        {
            if (maxTime == 0f) return 0f;
            return elapsed / maxTime;
        }
    }

    public OnTimerRunningEnd endCmd = null;

    public virtual void StartTimer(float time)
    {
        this.maxTime = time;

        if (this.started)
        {
            this.elapsed = 0f;
        }
        else
        {
            this.jumping = 1;
            this.started = true;
        }
    }

    public virtual void SetEndCmd(OnTimerRunningEnd cmd)
    {
        this.endCmd = cmd;
    }

    public virtual bool RunTimer()
    {
        if (!this.started)
        {
            return false;
        }
        elapsed += Time.deltaTime;
        if (elapsed >= maxTime)
        {
            this.jumping = 0;
            if (autoStop)
            {
                elapsed = 0f;
                started = false;

                if (endCmd != null)
                {
                    endCmd();
                    endCmd = null;
                }
            }
            return true;
        }
        return false;
    }

    public virtual void StartTimer()
    {
        this.maxTime = float.MaxValue;
        this.jumping = 1;

        if (this.started)
        {
            this.elapsed = 0f;
        }
        else
            this.started = true;
    }

    public virtual void StopTimer()
    {
        this.started = false;
        this.elapsed = 0f;
        this.maxTime = 0f;
    }

    public override string ToString()
    {
        return base.ToString() + "::[State]" + started + " [Max Time]" + maxTime + " [Elapsed]" + elapsed;
    }

    public virtual float GetRate()
    {
        try
        {
            return elapsed / maxTime;
        }
        catch (System.ArithmeticException e)
        {
#if UNITY_EDITOR
            Debug.Log(e);
#endif
            return 1f;
        }
    }
}

public static class GameStaticInfo {
    /// <summary>
    /// Value of  x / y (2)
    /// </summary>
    public const float VerticalRatio = 2f;

    /// <summary>
    /// Value of  y / x (0.5)
    /// </summary>
    public const float HorizontalRatio = 0.5f;

    /// <summary>
    /// Value of  z / x (0.2)
    /// </summary>
    public const float CrossRatio = 0.2f;

    /// <summary>
    /// 게임에서 사용되는 타일의 가로 길이
    /// </summary>
    public const float TileWidth = 1f;

    /// <summary>
    /// 게임에서 사용되는 타일의 가로 길이의 절반
    /// </summary>
    public const float TileWidth_Half = 0.5f;

    /// <summary>
    /// 게임에서 사용되는 타일의 세로 높이
    /// </summary>
    public const float TileHeight = 0.5f;
    /// <summary>
    /// 게임에서 사용되는 타일의 세로 높이의 절반
    /// </summary>
    public const float TileHeight_Half = 0.25f;

    /// <summary>
    /// 게임에서 사용되는 타일의 y 축 기준점->플레이어 유닛, NPC등에 사용될 것
    /// </summary>
    public const float ZeroHeight = 0.35f;

    /// <summary>
    /// 이동속도 배수
    /// </summary>
    public const float GameSpeedFactor = 0.01f;
}

public class PlayerMoveController : MonoBehaviour {
    public float moveSpeed = 1f;
    public float jumpDelay = 1f;
    public Animator PlayerMovementCTRL;
    public Transform FlipPivot;
	public static PlayerMoveController Player{ get; private set;}

    public float MaxHealth = 100f;

    public float health = 100f;
    public float stamina = 100f;

    public RenderLayerChanger renderLayerChanger;
    TileUnit currentTile = null;

    float CurrentPivotXScale { get { return FlipPivot.transform.localScale.x; } }

    Timer _jumpDelayTimer = new Timer();
    Timer _heightChangeTimer = new Timer();
    public float _heightChangeTime = 0.5f;
    float _targetHeight = 0f;
    float _initialHeight = 0f;

    float _movableSpace_x = 0f;
    float _movableSpace_y = 0f;

	void Awake(){

		Player = this;

	}

    private void Start()
    {
        //transform.position = new Vector3(0f, GameStaticInfo.ZeroHeight);

        //_movableSpace_x = MapGenerator.Instance.Width * 0.5f;
        //_movableSpace_y = MapGenerator.Instance.Height * 0.25f;

        currentTile = RandomMapGenerator.Instance.GetTile(transform.position);
    }

    void CalculateSortingOrder(TileUnit tile) {
        //List<TileUnit> ary = new List<TileUnit>();
        //int min, max, cur;
        //cur = min = max = tile.GetSpriteOrder();

        //for (int i = -1; i < 2; i++)
        //{
        //    for (int j = -1; j < 2; j++) {
        //        if (i == j) continue;

        //        TileUnit tu = RandomMapGenerator.Instance.GetTile(tile.x + i, tile.y + j);
        //        if (tu == null) continue;
        //        int order = tu.GetSpriteOrder();

        //        if (i < j)
        //        {
        //            if (max <= order)
        //            {
        //                max = order;
        //            }
        //        }
        //        else if (i > 1) {
        //            if (min >= order) {
        //                min = order;
        //            }
        //        }
        //    }
        //}

        //Debug.Log(string.Format("Current : {0}\tMin : {1}\tMax : {2}", cur, min, max));
        //int so = Mathf.Max(cur, max);
        //so = Mathf.Min(so, min);
        renderLayerChanger.ReferenceRenderer.sortingOrder = tile.GetSpriteOrder() + 1;
        renderLayerChanger.UpdateLayerInfo();


    }

    void Update() {
        var tile = RandomMapGenerator.Instance.GetTile(transform.position);
        if (tile != currentTile) {
            currentTile = tile;
            //renderLayerChanger.ReferenceRenderer.sortingOrder = currentTile.GetSpriteOrder() + 1;
            //renderLayerChanger.UpdateLayerInfo();
            CalculateSortingOrder(currentTile);
            _targetHeight = currentTile.HeightLevel * 0.25f;
            _initialHeight = FlipPivot.transform.localPosition.y;
            _heightChangeTimer.StartTimer(_heightChangeTime);
        }

        if (_heightChangeTimer.started) {
            float rate = Mathf.Lerp(_initialHeight, _targetHeight, _heightChangeTimer.Rate);
            var lp = FlipPivot.transform.localPosition;
            if (_heightChangeTimer.RunTimer()) {
                rate = _targetHeight;
            }
            lp.y = rate;
            FlipPivot.transform.localPosition = lp;
        }

        Vector3 currentPos = transform.position;
        Vector3 movePos = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            //currentPos.y += moveSpeed;
            MoveUp(ref movePos);
        }

        if (Input.GetKey(KeyCode.S))
        {
            MoveDown(ref movePos);
        }

        if (Input.GetKey(KeyCode.A))
        {
            MoveLeft(ref movePos);
        }

        if (Input.GetKey(KeyCode.D))
        {
            MoveRight(ref movePos);
        }

        Vector3 newPos = currentPos;
        newPos.x = Mathf.Clamp(newPos.x + movePos.x, -_movableSpace_x, _movableSpace_x);
        newPos.y = Mathf.Clamp(newPos.y + movePos.y, -_movableSpace_y + GameStaticInfo.TileHeight, _movableSpace_y);

        //transform.localPosition = newPos;

        if (movePos != Vector3.zero)
        {
            AnimatorUtil.SetBool(PlayerMovementCTRL, "Move", true);
        }
        else {
            AnimatorUtil.SetBool(PlayerMovementCTRL, "Move", false);
        }

        if (movePos.x > 0f)
        {
            if (CurrentPivotXScale < 0f)
            {
                Flip();
            }
        }
        else if (movePos.x < 0f)
        {
            if (CurrentPivotXScale > 0f)
            {
                Flip();
            }
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            Jump();
        }

        if (_jumpDelayTimer.started) {
            _jumpDelayTimer.RunTimer();
        }
    }

    void Flip() {
        var scale = FlipPivot.transform.localScale;
        scale.x *= -1f;
        FlipPivot.transform.localScale = scale;
    }

    /// <summary>
    /// Player Jump
    /// </summary>
    /// <param name="input">사용자 입력에 의한 점프인가</param>
    void Jump(bool input = true) {
        //position update needed?

        if (input && _jumpDelayTimer.started) return;

        AnimatorUtil.SetTrigger(PlayerMovementCTRL, "Jump");

        if (input) {
            _jumpDelayTimer.StartTimer(jumpDelay);
        }
    }

    public Vector3 GetCurrentPos()
    {
        return transform.position;
    }
    //deltaTime : 프레임에 렉이 걸린만큼 값이 커져 프레임렉을 보정
    void MoveUp(ref Vector3 pos)
    {
        pos.y += moveSpeed * Time.deltaTime * GameStaticInfo.HorizontalRatio;
        //RandomMapGenerator.Instance.GetDepthByCoor(transform.position[0],transform.position[1]);
        if (RandomMapGenerator.Instance.GetDepth(transform.position + pos) - RandomMapGenerator.Instance.GetDepth(transform.position)
            - _jumpDelayTimer.jumping < 2)
        {
            transform.Translate(0, moveSpeed * Time.deltaTime * GameStaticInfo.HorizontalRatio, 0);
        }
    }

    void MoveDown(ref Vector3 pos)
    {
        pos.y -= moveSpeed * Time.deltaTime * GameStaticInfo.HorizontalRatio;
        if (RandomMapGenerator.Instance.GetDepth(transform.position + pos) - RandomMapGenerator.Instance.GetDepth(transform.position)
            - _jumpDelayTimer.jumping < 2)
        {
            transform.Translate(0, -moveSpeed * Time.deltaTime * GameStaticInfo.HorizontalRatio, 0);
        }
    }

        void MoveLeft(ref Vector3 pos)
    {
        pos.x -= moveSpeed * Time.deltaTime;
        if (RandomMapGenerator.Instance.GetDepth(transform.position + pos) - RandomMapGenerator.Instance.GetDepth(transform.position)
                - _jumpDelayTimer.jumping < 2)
        {
            transform.Translate(-moveSpeed * Time.deltaTime, 0, 0);
        }
    }

    void MoveRight(ref Vector3 pos)
    {
        pos.x += moveSpeed * Time.deltaTime;
        if (RandomMapGenerator.Instance.GetDepth(transform.position + pos) - RandomMapGenerator.Instance.GetDepth(transform.position)
            - _jumpDelayTimer.jumping < 2)
        {
            transform.Translate(moveSpeed * Time.deltaTime, 0, 0);
        }
    }
}