using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActionFlow { 

    public interface INode
    {
    }

    /// <summary>
    /// 有状态的节点。该节点会保留状态，通常是一个持续运行的节点
    /// </summary>
    public unsafe interface IStatusNode : INode
    {
        void OnTick(ref Context context);
        Type NodeDataType();
        int CreateNodeDataTo(byte* b);
    }

    public interface IStatusNode<T> : IStatusNode
    {

    }


    public interface INodeAsset
    {
        INode GetValue();
    }

    /// <summary>
    /// 会进入Sleep状态的对象
    /// </summary>
    public interface ISleepable
    {
        void Wake(ref Context context);
    }

    /// <summary>
    /// 黑板数据的注入接口。实际请使用IAccessBlackboard子接口 TODO:换C#8.0时可提供默认实现
    /// 实现代码:builder.Add<T>();
    /// </summary>
    public interface IAccessBlackboard
    {
        void ToBuilder(NativeStaticMapHead.Builder builder);
    }

    public interface IGetBlackboard<T> : IAccessBlackboard where T:struct
    {
    }
    public interface ISetBlackboard<T> : IAccessBlackboard where T : struct
    {
    }
    public interface IGetSetBlackboard<T> : IGetBlackboard<T>, ISetBlackboard<T> where T:struct
    {
    }



    #region 输入输出

    /// <summary>
    /// 节点的输入接口定义
    /// </summary>
    /// <typeparam name="TData">接受的参数</typeparam>
    public interface INodeInput<TData> where TData : struct
    {
        void OnInput(ref Context context,TData inputData);
    }

    /// <summary>
    /// 节点的输入接口定义 无参数
    /// </summary>
    public interface INodeInput
    {
        void OnInput(ref Context context);
    }

    /// <summary>
    /// 做为参数来源的节点。该节点会被其它节点调用以获得一些数据，如环境数据
    /// </summary>
    /// <typeparam name="T">返回的值类型</typeparam>
    public interface IParameterType<T>
    {
        T GetValue(ref Context context, int nodeIndex);
    }
    #endregion


    #region 行为树节点
    public enum BehaviorStatus
    {
        None = 0,
        Success = 1,
        Failure = 2,
        Running = 3,
    }

    /// <summary>
    /// 行为树节点中列表的下一个的信息
    /// </summary>
    public struct NextNodeInfo
    {
        public int ChildIndex; //节点在列表中的索引
        public int ArrayIndex; //节点的顺序索引
        public bool Valid; //列表为空
        public bool End; //已经没有下一个节点
    }
    

    /// <summary>
    /// 行为树节点
    /// </summary>
    public interface IBehaviorNode
    {
        /// <summary>
        /// 行为树Node的输入端口
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        BehaviorStatus BehaviorInput(ref Context context);

    }

    /// <summary>
    /// 行为树中的流程节点。继承IBehaviorNode带输出逻辑
    /// </summary>
    public interface IBehaviorCompositeNode:IBehaviorNode
    {
        /// <summary>
        /// 当前Node的子节点从Running状态结束时调用,作用是子节点运行完成时候返回的通知
        /// </summary>
        /// <param name="context"></param>
        /// <param name="index">Running结束的Node的索引</param>
        /// <returns>true则继续向上传递</returns>
        (bool, BehaviorStatus) Completed(ref Context context, int childIndex, BehaviorStatus result);
    }

    #endregion

}
