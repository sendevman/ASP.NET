﻿using System.ComponentModel.DataAnnotations;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Dependency;
using Abp.Runtime.Validation;
using Shouldly;
using Xunit;

namespace Abp.TestBase.Tests.Application.Services
{
    public class Validation_Tests : AbpIntegratedTestBase
    {
        private readonly IMyAppService _myAppService;

        public Validation_Tests()
        {
            LocalIocManager.Register<IMyAppService, MyAppService>(DependencyLifeStyle.Transient);
            _myAppService = LocalIocManager.Resolve<IMyAppService>();
        }

        [Fact]
        public void Should_Work_Proper_With_Right_Inputs()
        {
            var output = _myAppService.MyMethod(new MyMethodInput { MyStringValue = "test" });
            output.Result.ShouldBe(42);
        }

        [Fact]
        public void Should_Not_Work_Proper_With_Wrong_Inputs()
        {
            Assert.Throws<AbpValidationException>(() => _myAppService.MyMethod(new MyMethodInput())); //MyStringValue is not supplied!
            Assert.Throws<AbpValidationException>(() => _myAppService.MyMethod(new MyMethodInput { MyStringValue = "a" })); //MyStringValue's min length should be 3!
        }

        [Fact]
        public void Should_Work_Proper_With_Right_Nesned_Inputs()
        {
            var output = _myAppService.MyMethod2(new MyMethod2Input
                            {
                                MyStringValue2 = "test 1",
                                Input1 = new MyMethodInput {MyStringValue = "test 2"}
                            });
            output.Result.ShouldBe(42);
        }

        [Fact]
        public void Should_Not_Work_With_Wrong_Nesned_Inputs_1()
        {
            Assert.Throws<AbpValidationException>(() =>
                _myAppService.MyMethod2(new MyMethod2Input
                {
                    MyStringValue2 = "test 1",
                    Input1 = new MyMethodInput() //MyStringValue is not set
                })); 
        }

        [Fact]
        public void Should_Not_Work_With_Wrong_Nesned_Inputs_2()
        {
            Assert.Throws<AbpValidationException>(() =>
                _myAppService.MyMethod2(new MyMethod2Input //Input1 is not set
                                        {
                                            MyStringValue2 = "test 1"
                                        }));
        }

        #region Nested Classes

        public interface IMyAppService
        {
            MyMethodOutput MyMethod(MyMethodInput input);
            MyMethodOutput MyMethod2(MyMethod2Input input);
        }

        public class MyAppService : IMyAppService, IApplicationService
        {
            public MyMethodOutput MyMethod(MyMethodInput input)
            {
                return new MyMethodOutput { Result = 42 };
            }

            public MyMethodOutput MyMethod2(MyMethod2Input input)
            {
                return new MyMethodOutput { Result = 42 };
            }
        }

        public class MyMethodInput : IInputDto
        {
            [Required]
            [MinLength(3)]
            public string MyStringValue { get; set; }
        }

        public class MyMethod2Input : IInputDto
        {
            [Required]
            [MinLength(2)]
            public string MyStringValue2 { get; set; }

            [Required]
            public MyMethodInput Input1 { get; set; }
        }

        public class MyMethodOutput : IOutputDto
        {
            public int Result { get; set; }
        }

        #endregion
    }
}
