/**
 *Copyright(C) 2015 by DefaultCompany
 *All rights reserved.
 *FileName:     Singleton.cs
 *Author:       若飞
 *Version:      1.0
 *UnityVersion：5.3.2f1
 *Date:         2017-04-25
 *Description:
 *History:
*/

/// <summary>
/// 单例类型的基类
/// </summary>
/// <typeparam name="T">类型</typeparam>
public abstract class Singleton<T> where T : class, new()
{
    private static T _instance;
    private static readonly object locker = new object();

    protected Singleton()
    {
        Init();
    }

    protected virtual void Init()
    {
    }

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (locker)
                {
                    if (_instance == null)
                    {
                        _instance = new T();
                    }
                }
            }

            return _instance;
        }
    }
}