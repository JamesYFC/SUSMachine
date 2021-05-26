using NUnit.Framework;
using Moq;

namespace KDMagical.SUSMachine.Tests
{
    public partial class SUSMachineTests
    {
        internal class StateBehaviourTests
        {
            [Test, AutoMoqData]
            public void StateBehaviour_Calls_Actions(IStateMachine<States> stateMachine)
            {
                var actions = new MockStateActions();
                var (enter, exit, update, fixedUpdate, lateUpdate) = actions;

                var sut = new StateBehaviour<States>
                {
                    OnEnter = enter.Object,
                    OnExit = exit.Object,
                    OnUpdate = update.Object,
                    OnFixedUpdate = fixedUpdate.Object,
                    OnLateUpdate = lateUpdate.Object
                };

                sut.Initialize(stateMachine);

                sut.DoEnter();

                enter.Verify(f => f(stateMachine), Times.Once);
                exit.Verify(f => f(It.IsAny<IStateMachine<States>>()), Times.Never);
                update.Verify(f => f(It.IsAny<IStateMachine<States>>()), Times.Never);
                fixedUpdate.Verify(f => f(It.IsAny<IStateMachine<States>>()), Times.Never);
                lateUpdate.Verify(f => f(It.IsAny<IStateMachine<States>>()), Times.Never);

                sut.DoExit();

                enter.Verify(f => f(stateMachine), Times.Once);
                exit.Verify(f => f(stateMachine), Times.Once);
                update.Verify(f => f(It.IsAny<IStateMachine<States>>()), Times.Never);
                fixedUpdate.Verify(f => f(It.IsAny<IStateMachine<States>>()), Times.Never);
                lateUpdate.Verify(f => f(It.IsAny<IStateMachine<States>>()), Times.Never);

                sut.DoUpdate();

                enter.Verify(f => f(stateMachine), Times.Once);
                exit.Verify(f => f(stateMachine), Times.Once);
                update.Verify(f => f(stateMachine), Times.Once);
                fixedUpdate.Verify(f => f(It.IsAny<IStateMachine<States>>()), Times.Never);
                lateUpdate.Verify(f => f(It.IsAny<IStateMachine<States>>()), Times.Never);

                sut.DoFixedUpdate();

                enter.Verify(f => f(stateMachine), Times.Once);
                exit.Verify(f => f(stateMachine), Times.Once);
                update.Verify(f => f(stateMachine), Times.Once);
                fixedUpdate.Verify(f => f(stateMachine), Times.Once);
                lateUpdate.Verify(f => f(It.IsAny<IStateMachine<States>>()), Times.Never);

                sut.DoLateUpdate();

                enter.Verify(f => f(stateMachine), Times.Once);
                exit.Verify(f => f(stateMachine), Times.Once);
                update.Verify(f => f(stateMachine), Times.Once);
                fixedUpdate.Verify(f => f(stateMachine), Times.Once);
                lateUpdate.Verify(f => f(stateMachine), Times.Once);
            }
        }
    }
}