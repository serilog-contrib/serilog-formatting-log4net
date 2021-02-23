using System;
using System.Globalization;
using System.Xml;
using FluentAssertions;
using Xunit;

namespace Serilog.Formatting.Log4Net.Tests
{
    public class Log4NetTextFormatterOptionsBuilderTest
    {
        [Fact]
        public void UseFormatProvider()
        {
            // Arrange
            var formatProvider = NumberFormatInfo.InvariantInfo;

            // Act
            var builder = new Log4NetTextFormatterOptionsBuilder().UseFormatProvider(formatProvider);

            // Assert
            builder.FormatProvider.Should().BeSameAs(formatProvider);
        }

        [Theory]
        [InlineData(CDataMode.Always)]
        [InlineData(CDataMode.Never)]
        [InlineData(CDataMode.IfNeeded)]
        public void UseCDataMode(CDataMode cDataMode)
        {
            // Act
            var builder = new Log4NetTextFormatterOptionsBuilder().UseCDataMode(cDataMode);

            // Assert
            builder.CDataMode.Should().Be(cDataMode); // BeSameAs doesn't work for enums
        }

        [Fact]
        public void UseLog4NetXmlNamespace()
        {
            // Arrange
            var xmlNamespace = new XmlQualifiedName("test", "test");

            // Act
            var builder = new Log4NetTextFormatterOptionsBuilder().UseLog4NetXmlNamespace(xmlNamespace);

            // Assert
            builder.Log4NetXmlNamespace.Should().BeSameAs(xmlNamespace);
        }

        [Theory]
        [InlineData(LineEnding.LineFeed)]
        [InlineData(LineEnding.CarriageReturn)]
        [InlineData(LineEnding.LineFeed | LineEnding.CarriageReturn)]
        public void UseLineEnding(LineEnding lineEnding)
        {
            // Act
            var builder = new Log4NetTextFormatterOptionsBuilder().UseLineEnding(lineEnding);

            // Assert
            builder.LineEnding.Should().Be(lineEnding); // BeSameAs doesn't work for enums
        }

        [Fact]
        public void UseIndentationSettings()
        {
            // Arrange
            var indentationSettings = new IndentationSettings(indentation: default, size: 1);

            // Act
            var builder = new Log4NetTextFormatterOptionsBuilder().UseIndentationSettings(indentationSettings);

            // Assert
            builder.IndentationSettings.Should().BeSameAs(indentationSettings);
        }

        [Fact]
        public void UseNoIndentation()
        {
            // Act
            var builder = new Log4NetTextFormatterOptionsBuilder().UseNoIndentation();

            // Assert
            builder.IndentationSettings.Should().BeNull();
        }

        [Fact]
        public void UsePropertyFilter()
        {
            // Arrange
            PropertyFilter filterProperty = (logEvent, propertyName) => true;

            // Act
            var builder = new Log4NetTextFormatterOptionsBuilder().UsePropertyFilter(filterProperty);

            // Assert
            builder.FilterProperty.Should().BeSameAs(filterProperty);
        }

        [Fact]
        public void SettingPropertyFilterToNullThrowsArgumentNullException()
        {
            // Act
            Func<Log4NetTextFormatterOptionsBuilder> action = () => new Log4NetTextFormatterOptionsBuilder().UsePropertyFilter(null!);

            // Assert
            action.Should().ThrowExactly<ArgumentNullException>()
                .WithMessage("The FilterProperty option can not be null.*")
                .And.ParamName.Should().Be("filterProperty");
        }

        [Fact]
        public void UseExceptionFormatter()
        {
            // Arrange
            ExceptionFormatter formatException = exception => "";

            // Act
            var builder = new Log4NetTextFormatterOptionsBuilder().UseExceptionFormatter(formatException);

            // Assert
            builder.FormatException.Should().BeSameAs(formatException);
        }

        [Fact]
        public void UseLog4JCompatibility()
        {
            // Arrange
            var builder = new Log4NetTextFormatterOptionsBuilder();

            // Act
            builder.UseLog4JCompatibility();

            // Assert
            builder.LineEnding.Should().Be(LineEnding.CarriageReturn | LineEnding.LineFeed);
            builder.CDataMode.Should().Be(CDataMode.Always);
            builder.Log4NetXmlNamespace.Should().NotBeNull();
            builder.Log4NetXmlNamespace!.Name.Should().Be("log4j");
        }

        [Fact]
        public void SettingExceptionFormatterToNullThrowsArgumentNullException()
        {
            // Act
            Func<Log4NetTextFormatterOptionsBuilder> action = () => new Log4NetTextFormatterOptionsBuilder().UseExceptionFormatter(null!);

            // Assert
            action.Should().ThrowExactly<ArgumentNullException>()
                .WithMessage("The FormatException option can not be null.*")
                .And.ParamName.Should().Be("formatException");
        }
    }
}