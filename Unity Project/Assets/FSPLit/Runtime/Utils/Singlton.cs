/********************************************************
 * FileName:    Singlton.cs
 * Description: 单例模板    
 ********************************************************/
using System;
using System.Collections.Generic;

namespace Nave.FSPLit
{
    /// <summary>
    /// 单例设计
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> where T : class // ,new()不支持非公共的无参构造函数 
    {
        private static T _instance = null;
        private static readonly object syslock = new object();

        public static T Instance
        {
            get
            {   //没有第一重 singleton == null 的话，每一次有线程进入Instance，均会执行锁定操作来实现线程同步，
                //非常耗费性能 增加第一重singleton ==null 成立时的情况下执行一次锁定以实现线程同步
                if (_instance == null)
                {
                    //线程锁定，防止出现多线程同时访问该数据而出错
                    lock (syslock)
                    {
                        //Double-Check Locking 双重检查锁定
                        if (_instance == null)
                        {
                            //需要非公共的无参构造函数，不能使用new T() ,new不支持非公共的无参构造函数 
                            //第二个参数防止异常：“没有为该对象定义无参数的构造函数。”
                            _instance = (T)Activator.CreateInstance(typeof(T), true);
                        }
                    }
                }
                return _instance;
            }
        }

        public virtual void Init()
        {

        }

        public virtual void Release()
        {

        }
    }
}


