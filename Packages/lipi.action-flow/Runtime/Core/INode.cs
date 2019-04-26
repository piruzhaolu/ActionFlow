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
        Success = 1,
        Failure = 2,
        Running = 3
    }


    public interface IBehaviorNode
    {
        /// <summary>
        /// 行为树Node的输入端口
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        BehaviorStatus BehaviorInput(ref Context context);

        /// <summary>
        /// 当前Node的子节点从Running状态结束时调用
        /// </summary>
        /// <param name="context"></param>
        /// <param name="index">Running结束的Node的索引</param>
        void Completed(ref Context context, int index);
    }

    #endregion

}
