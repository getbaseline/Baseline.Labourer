using System;
using FluentAssertions;
using Xunit;

namespace Baseline.Labourer.Server.Tests
{
    public class RetryConfigurationTests
    {
        [Fact]
        public void It_Throws_An_Exception_If_An_Incorrect_Number_Of_Delays_Are_Provided()
        {
            // Act.
            Action act = () => new RetryConfiguration(3, new[] {TimeSpan.Zero, TimeSpan.Zero});
            
            // Assert.
            act.Should().ThrowExactly<ArgumentException>();
        }
    }
}