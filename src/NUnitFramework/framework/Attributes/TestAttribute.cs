// ***********************************************************************
// Copyright (c) 2014 Charlie Poole, Rob Prouse
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

#nullable enable

using System;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;

namespace NUnit.Framework
{
    /// <summary>
    /// Marks the method as callable from the NUnit test runner.
    /// </summary>
    ///
    /// <example>
    /// [TestFixture]
    /// public class Fixture
    /// {
    ///   [Test]
    ///   public void MethodToTest()
    ///   {}
    ///
    ///   [Test(Description = "more detailed description")]
    ///   public void TestDescriptionMethod()
    ///   {}
    /// }
    /// </example>
    ///
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class TestAttribute : NUnitAttribute, ISimpleTestBuilder, IApplyToTest, IImplyFixture
    {
        private object? _expectedResult;

        private readonly NUnitTestCaseBuilder _builder = new NUnitTestCaseBuilder();

        /// <summary>
        /// Gets or sets the name of the test.
        /// </summary>
        /// <value>The name of the test.</value>
        public string? TestName { get; set; }

        /// <summary>
        /// Descriptive text for this test
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The author of this test
        /// </summary>
        public string? Author { get; set; }

        /// <summary>
        /// The type that this test is testing
        /// </summary>
        public Type? TestOf { get; set; }

        /// <summary>
        /// Returns true if the expected result has been set
        /// </summary>
        public bool HasExpectedResult { get; private set; }

        /// <summary>
        /// Gets or sets the expected result. Not valid if the test
        /// method has parameters.
        /// </summary>
        /// <value>The result.</value>
        public object? ExpectedResult
        {
            get { return _expectedResult; }
            set
            {
                _expectedResult = value;
                HasExpectedResult = true;
            }
        }

        #region IApplyToTest Members

        /// <summary>
        /// Modifies a test by adding a description, if not already set.
        /// </summary>
        /// <param name="test">The test to modify</param>
        public void ApplyToTest(Test test)
        {
            if (!test.Properties.ContainsKey(PropertyNames.Description) && Description != null)
                test.Properties.Set(PropertyNames.Description, Description);

            if (!test.Properties.ContainsKey(PropertyNames.Author) && Author != null)
                test.Properties.Set(PropertyNames.Author, Author);

            if (!test.Properties.ContainsKey(PropertyNames.TestOf) && TestOf != null)
                test.Properties.Set(PropertyNames.TestOf, TestOf.FullName);

            if (HasExpectedResult && test.Method.GetParameters().Length > 0)
                test.MakeInvalid("The 'TestAttribute.ExpectedResult' property may not be used on parameterized methods.");

        }

        #endregion

        #region ISimpleTestBuilder Members

        /// <summary>
        /// Builds a single test from the specified method and context.
        /// </summary>
        /// <param name="method">The method for which a test is to be constructed.</param>
        /// <param name="suite">The suite to which the test will be added.</param>
        public TestMethod BuildFrom(IMethodInfo method, Test suite)
        {
            TestCaseParameters? parms = new TestCaseParameters();
            parms.TestName = TestName;

            // Special handling for ExpectedResult (see if it needs to be converted into method return type)
            if (HasExpectedResult
                && ParamAttributeTypeConversions.TryConvert(ExpectedResult, method.ReturnType.Type, out var expectedResultInTargetType))
            {
                parms.ExpectedResult = expectedResultInTargetType;
            }

            return _builder.BuildTestMethod(method, suite, parms);
        }

        #endregion
    }
}
