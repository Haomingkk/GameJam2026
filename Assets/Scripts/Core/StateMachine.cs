using System;
using System.Collections.Generic;

namespace GameJam26.FSM
{

    public interface IState<TContext>
    {
        string Name { get; }
        void OnEnter(TContext context);
        void OnExit(TContext context);
        void Tick(TContext context, float deltaTime);
    }

    public interface ITransition<TContext>
    {
        bool CanTransit(TContext context);
        string Reason { get; }
    }

    public class PredTransition<TContext> : ITransition<TContext>
    {
        private readonly Func<TContext, bool> _pred;
        public string Reason { get; }

        public PredTransition(Func<TContext, bool> pred, string reason)
        {
            _pred = pred;
            Reason = reason;
        }

        public bool CanTransit(TContext context) => _pred(context);

    }

    public class StateMachine<TContext>
    {
        public IState<TContext> Current { get; private set; }
        public string LastTransitionReason { get; private set; }
        public string LastFrom { get; private set; }
        public string LastTo { get; private set; }

        //普通状态转移表
        private readonly Dictionary<IState<TContext>, List<(ITransition<TContext> cond, IState<TContext> to)>> _graph = new();
        //全局状态转移表
        private readonly List<(ITransition<TContext> cond, IState<TContext> to)> _any = new();

        public void AddTransition(IState<TContext> from, IState<TContext> to, ITransition<TContext> cond)
        {
            if (!_graph.TryGetValue(from, out var list))
            {
                _graph[from] = list = new List<(ITransition<TContext>, IState<TContext>)>();
            }
            list.Add((cond, to));
        }

        public void AddAnyTransition(IState<TContext> to, ITransition<TContext> cond)
        {
            _any.Add((cond, to));
        }

        public void SetInitialState(IState<TContext> state, TContext context)
        {
            Current = state;
            Current.OnEnter(context);
        }

        public void Tick(TContext context, float deltaTime)
        {
            if (_TryGetNext(context, out var next, out var reason))
            {
                _SwitchTo(next, context, reason);
            }
            Current?.Tick(context, deltaTime);
        }

        #region private methods
        private bool _TryGetNext(TContext context, out IState<TContext> next, out string reason)
        {
            foreach (var (cond, to) in _any)
            {
                if (cond.CanTransit(context))
                {
                    next = to;
                    reason = cond.Reason;
                    return true;
                }
            }
            if (Current != null && _graph.TryGetValue(Current, out var transitions))
            {
                foreach (var (cond, to) in transitions)
                {
                    if (cond.CanTransit(context))
                    {
                        next = to;
                        reason = cond.Reason;
                        return true;
                    }
                }
            }
            next = null;
            reason = null;
            return false;
        }

        private void _SwitchTo(IState<TContext> next, TContext context, string reason)
        {
            if (next == Current) return;
            LastFrom = Current?.Name;
            LastTo = next?.Name;
            LastTransitionReason = reason;
            Current?.OnExit(context);
            Current = next;
            Current.OnEnter(context);
        }

        #endregion
    }
}


