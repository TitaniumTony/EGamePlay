using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class TestOdin : MonoBehaviour
{
    [Title("测试标题")]
    [TableList]
    public List<Config> configs = new List<Config>()
    {
        new Config() { Id = 1, groupId = 1, name = "卡牌" }
    };

    private void Start()
    {
        configs = new List<Config>()
        {
            new Config() { Id = 1, groupId = 1, name = "卡牌" }
        };
    }
}

[System.Serializable]
public class Config
{
    public int Id;
    public int GroupId;
    [LabelText("卡排名")]public string name; // 卡牌名
    public string drawPath; // 卡牌资源
    
    [VerticalGroup("VerticalGroup")]
    public int cardType,groupId,wave; // ID组
    //public int wave; // 波数
    
    // public string stageRange; // 层数区间
    // public int stageType; // 层数类型
    // public string rewardString; // 掉落表ID,数量下限，数量上限
    // public bool isHide; // 
    // public string foodString;
    // public bool IsEvent;//是否是事件元素
    // public int LockType;//上锁类型
    // public string LockTip;//解锁文字提示
    // public int ForceUnlock;//强行获取  EventConfig表ID组
}

