﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType {
    WATER,
    GROUND_WALKABLE,
    GROUND_UNWALKABLE
}

public class TileUnit : MonoBehaviour {
    public const float _DEF_HEIGHT = -0.5f + 0.135f;
    public TileType type;
    public TempTileModel _model;
    public SpriteRenderer spriteRenderer;
    public new Collider2D collider;

    public int HeightLevel = 0;
    public Vector3 originalPosition;
    public bool isNearWater = false;

    public int x = 0, y = 0;

    public void SetModel(TempTileModel model) {
        _model = model;
        //spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        //GameObject _text = new GameObject(_model.xPos + ", " + _model.yPos);
        //_text.transform.SetParent(RandomMapGenerator.Instance.gameObject.GetComponentInChildren<RectTransform>());
        //_text.transform.localScale = Vector3.one;
        //_text.transform.localRotation = Quaternion.Euler(Vector3.zero);
        //_text.transform.localPosition = 
    }

    public void SetPosition(Vector3 pos) {
        originalPosition = pos;
        transform.localPosition = pos;
        Vector3 colPos = collider.transform.position;
        colPos.z = RandomMapGenerator.Instance.transform.position.z;
        collider.transform.position = colPos;
    }

    public void SetCoord(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public void SetHeight(int heightLevel) {
        HeightLevel = heightLevel;
        spriteRenderer.transform.localPosition = new Vector3(0, 0.25f * heightLevel + _DEF_HEIGHT, 0f);

        spriteRenderer.sortingOrder = -(x - y) * RandomMapGenerator.SPRITE_ORDER_INTERVAL + HeightLevel;
        //_text.text = GetSpriteOrder().ToString();
    }

    public int GetSpriteOrder() { return spriteRenderer.sortingOrder; }
}
