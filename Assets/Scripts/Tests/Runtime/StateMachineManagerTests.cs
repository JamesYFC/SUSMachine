using System.Collections;
using System.Collections.Generic;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace KDMagical.SUSMachine.Tests
{
    public partial class SUSMachineTests
    {
        internal class StateMachineManagerTests
        {
            [UnityTest]
            public IEnumerator Register_During_Lifecycle_No_Error()
            {
                var fixture = new Fixture()
                    .Customize(new AutoMoqCustomization());

                Assert.IsTrue(StateMachineManager.Instance);

                var state = fixture.Create<States>();
                var newFsm = fixture.Create<IStateMachine>();

                var creatorFsm = new StateMachine<States, Events>
                {
                    [state] = {
                        OnUpdate = _ => StateMachineManager.Instance.Register(newFsm)
                    }
                };

                creatorFsm.Initialize(state);

                yield return null;

                Mock.Get(newFsm).Verify(fsm => fsm.DoUpdate(), Times.Once);
            }

            [UnityTest]
            public IEnumerator Deregister_During_Lifecycle_No_Error()
            {
                var fixture = new Fixture()
                    .Customize(new AutoMoqCustomization());

                Assert.IsTrue(StateMachineManager.Instance);

                var state = fixture.Create<States>();
                var deregisteringFsm = fixture.Create<IStateMachine>();

                var creatorFsm = new StateMachine<States, Events>
                {
                    [state] = {
                        OnUpdate = _ => StateMachineManager.Instance.Deregister(deregisteringFsm)
                    }
                };

                creatorFsm.Initialize(state);
                StateMachineManager.Instance.Register(deregisteringFsm);

                yield return null;

                Mock.Get(deregisteringFsm).Verify(deregisteringFsm => deregisteringFsm.DoUpdate(), Times.Never);
            }

            [UnityTest]
            public IEnumerator Deregister_During_Lifecycle_No_Call()
            {
                var fixture = new Fixture()
                    .Customize(new AutoMoqCustomization());

                Assert.IsTrue(StateMachineManager.Instance);

                var state = fixture.Create<States>();
                var fsm = fixture.Create<IStateMachine>();
                var fsmMock = Mock.Get(fsm);

                StateMachineManager.Instance.Register(fsm);

                yield return null;

                fsmMock.Verify(s => s.DoUpdate(), Times.Once);

                StateMachineManager.Instance.Deregister(fsm);

                yield return null;

                fsmMock.Verify(s => s.DoUpdate(), Times.Once);
            }
        }

    }
}
