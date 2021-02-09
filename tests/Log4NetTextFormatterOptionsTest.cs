using System;
using System.Linq;
using System.Reflection;
using System.Xml;
using FluentAssertions;
using Xunit;

namespace Serilog.Formatting.Log4Net.Tests
{
    /// <summary>
    /// Using reflection because I want Log4NetTextFormatterOptions to be internal and I don't want to use System.Runtime.CompilerServices.InternalsVisibleToAttribute
    /// </summary>
    public class Log4NetTextFormatterOptionsTest
    {
        private static ConstructorInfo Log4NetTextFormatterOptionsConstructor { get; } = typeof(Log4NetTextFormatter).Assembly.GetType("Serilog.Formatting.Log4Net.Log4NetTextFormatterOptions")!.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).Single();
        private static readonly PropertyFilter FilterProperty = (logEvent, propertyName) => true;
        private static readonly ExceptionFormatter FormatException = exception => "";

        [Fact]
        public void NullXmlWriterSettings_ThrowsArgumentNullException()
        {
            // Arrange
            IFormatProvider? formatProvider = default;
            const CDataMode cDataMode = default;
            XmlQualifiedName? log4NetXmlNamespace = default;
            XmlWriterSettings xmlWriterSettings = null!;
            var parameters = new object?[] {formatProvider, cDataMode, log4NetXmlNamespace, xmlWriterSettings, FilterProperty, FormatException};

            // Act
            Action action = () => Log4NetTextFormatterOptionsConstructor.Invoke(parameters);

            // Assert
            action.Should().ThrowExactly<TargetInvocationException>()
                .Which.InnerException.Should().BeOfType<ArgumentNullException>()
                .Which.ParamName.Should().Be("xmlWriterSettings");
        }

        [Fact]
        public void NullPropertyFilter_ThrowsArgumentNullException()
        {
            // Arrange
            IFormatProvider? formatProvider = default;
            const CDataMode cDataMode = default;
            XmlQualifiedName? log4NetXmlNamespace = default;
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            PropertyFilter filterProperty = null!;
            var parameters = new object?[] {formatProvider, cDataMode, log4NetXmlNamespace, xmlWriterSettings, filterProperty, FormatException};

            // Act
            Action action = () => Log4NetTextFormatterOptionsConstructor.Invoke(parameters);

            // Assert
            action.Should().ThrowExactly<TargetInvocationException>()
                .Which.InnerException.Should().BeOfType<ArgumentNullException>()
                .Which.ParamName.Should().Be("filterProperty");
        }

        [Fact]
        public void NullExceptionFormatter_ThrowsArgumentNullException()
        {
            // Arrange
            IFormatProvider? formatProvider = default;
            const CDataMode cDataMode = default;
            XmlQualifiedName? log4NetXmlNamespace = default;
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            ExceptionFormatter formatException = null!;
            var parameters = new object?[] {formatProvider, cDataMode, log4NetXmlNamespace, xmlWriterSettings, FilterProperty, formatException};

            // Act
            Action action = () => Log4NetTextFormatterOptionsConstructor.Invoke(parameters);

            // Assert
            action.Should().ThrowExactly<TargetInvocationException>()
                .Which.InnerException.Should().BeOfType<ArgumentNullException>()
                .Which.ParamName.Should().Be("formatException");
        }
    }
}