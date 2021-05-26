using Moq;
using NUnit.Framework;

namespace KDMagical.SUSMachine.Tests
{
    public partial class SUSMachineTests
    {
        internal class AutoTransitionTests
        {
            [Test, AutoMoqData]
            public void TransitionCheck_ShouldCall_AutoTransition_With_Params(
                IStateMachine<States> stateMachine)
            {
                var updateTransition = new Mock<AutoTransition<States>>();
                var fixedUpdateTransition = new Mock<AutoTransition<States>>();
                var lateUpdateTransition = new Mock<AutoTransition<States>>();

                var sut = new AutoTransitions<States>
                {
                    {updateTransition.Object, TransitionType.Update},
                    {fixedUpdateTransition.Object, TransitionType.FixedUpdate},
                    {lateUpdateTransition.Object, TransitionType.LateUpdate}
                };

                sut.Initialize(stateMachine);

                updateTransition.Verify(f => f(It.IsAny<IStateMachine<States>>()), Times.Never);
                fixedUpdateTransition.Verify(f => f(It.IsAny<IStateMachine<States>>()), Times.Never);
                lateUpdateTransition.Verify(f => f(It.IsAny<IStateMachine<States>>()), Times.Never);

                sut.CheckTransitions(TransitionType.Update);

                updateTransition.Verify(f => f(stateMachine), Times.Once);
                fixedUpdateTransition.Verify(f => f(It.IsAny<IStateMachine<States>>()), Times.Never);
                lateUpdateTransition.Verify(f => f(It.IsAny<IStateMachine<States>>()), Times.Never);

                sut.CheckTransitions(TransitionType.FixedUpdate);

                updateTransition.Verify(f => f(stateMachine), Times.Once);
                fixedUpdateTransition.Verify(f => f(stateMachine), Times.Once);
                lateUpdateTransition.Verify(f => f(It.IsAny<IStateMachine<States>>()), Times.Never);

                sut.CheckTransitions(TransitionType.LateUpdate);

                updateTransition.Verify(f => f(stateMachine), Times.Once);
                fixedUpdateTransition.Verify(f => f(stateMachine), Times.Once);
                lateUpdateTransition.Verify(f => f(stateMachine), Times.Once);
            }

            [Test, AutoMoqData]
            public void TransitionCheck_ShouldReturn_NextState(IStateMachine<States> stateMachine)
            {
                var sut = new AutoTransitions<States>
                {
                    {_ => States.State1, TransitionType.Update},
                    {_ => null, TransitionType.FixedUpdate}
                };

                sut.Initialize(stateMachine);

                Assert.AreEqual(sut.CheckTransitions(TransitionType.Update), States.State1);
                Assert.IsNull(sut.CheckTransitions(TransitionType.FixedUpdate));
                Assert.IsNull(sut.CheckTransitions(TransitionType.LateUpdate));
            }

            [Test, AutoMoqData]
            public void TransitionCheck_ShouldReturn_Null_On_NoTransitions(IStateMachine<States> stateMachine)
            {
                var sut = new AutoTransitions<States> { };

                sut.Initialize(stateMachine);

                Assert.IsNull(sut.CheckTransitions(TransitionType.Update));
                Assert.IsNull(sut.CheckTransitions(TransitionType.FixedUpdate));
                Assert.IsNull(sut.CheckTransitions(TransitionType.LateUpdate));
            }

            [Test, AutoMoqData]
            public void TransitionCheck_ShouldRespect_TransitionOrder(
                IStateMachine<States> stateMachine,
                TransitionType transitionType)
            {
                var sut = new AutoTransitions<States>
                {
                    {_ => States.State1, transitionType},
                    {_ => States.State2, transitionType},
                };

                sut.Initialize(stateMachine);

                Assert.AreEqual(sut.CheckTransitions(transitionType), States.State1);
            }

            [Test]
            [InlineAutoMoqData(-1, States.State1)]
            [InlineAutoMoqData(2, States.State2)]
            [InlineAutoMoqData(3, null)]
            public void TransitionCheck_SimpleForm_ShouldReturn_CorrectState(
                int num,
                States? expectedResult,
                TransitionType transitionType,
                IStateMachine<States> stateMachine)
            {
                var sut = new AutoTransitions<States>
                {
                    {_ => num < 0, States.State1, transitionType},
                    {_ => num % 2 == 0, States.State2, transitionType},
                };

                sut.Initialize(stateMachine);

                Assert.AreEqual(sut.CheckTransitions(transitionType), expectedResult);
            }

            [Test]
            [InlineAutoMoqData(-1, States.State1)]
            [InlineAutoMoqData(2, States.State2)]
            [InlineAutoMoqData(3, States.State3)]
            [InlineAutoMoqData(400, null)]
            public void TransitionCheck_ComplexForm_Conditionals_ShouldReturn_CorrectState(
                int num,
                States? expectedResult,
                TransitionType transitionType,
                IStateMachine<States> stateMachine)
            {
                var sut = new AutoTransitions<States>
                {
                    {_ => num < 0 ? (States?)States.State1 : null, transitionType},
                    {_ => {
                        if (num > 100)
                        {
                            return null;
                        }
                        return num % 2 == 0 ? States.State2 : States.State3;
                    }, transitionType},
                    {
                        _ => {
                            var someNum = 50;
                            switch (someNum)
                            {
                                case 1:
                                    return States.State1;
                                case 2:
                                    return States.State2;
                                case 3:
                                    return States.State3;
                                default:
                                    return null;
                            }
                        }
                    }
                };

                sut.Initialize(stateMachine);

                Assert.AreEqual(sut.CheckTransitions(transitionType), expectedResult);
            }
        }
    }
}
