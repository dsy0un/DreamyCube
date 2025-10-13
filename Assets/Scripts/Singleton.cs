using UnityEngine;

public class Singleton
{
    private static Singleton _instance;

    /** 
     * <summary>
     * �ʱ�ȭ : �̱��� ó�� ����� �� ȣ��
     * </summary>
     */
    private Singleton() {}

    public static Singleton GetInstance()
    {
        _instance ??= new Singleton();
        return _instance;
    }
}

public class Singleton<T> where T : new()
{
    protected static T _instance;

    protected static T GetInstance()
    {
        _instance ??= new T();
        return _instance;
    }
}