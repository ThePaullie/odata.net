﻿//---------------------------------------------------------------------
// <copyright file="OrderByBinderTests.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.Test.OData.Query.TDD.Tests.Semantic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using Microsoft.OData.Core.UriParser.Parsers;
    using Microsoft.OData.Edm;
    using Microsoft.OData.Core.UriParser.Semantic;
    using Microsoft.OData.Core.UriParser.Syntactic;
    using Microsoft.Test.OData.Query.TDD.Tests.TestUtilities;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.OData.Core.UriParser;

    /// <summary>
    /// Unit test for the OrderByBinder class.
    /// </summary>
    [TestClass]
    public class OrderByBinderTests
    {
        private static readonly ODataUriParserConfiguration configuration = new ODataUriParserConfiguration(HardCodedTestModel.TestModel);
        private BindingState bindingState;
        private OrderByBinder orderbyBinder;

        [TestInitialize]
        public void Init()
        {
            this.orderbyBinder = new OrderByBinder(FakeBindMethods.BindMethodReturningASinglePrimitive);

            var implicitRangeVariable = new EntityRangeVariable(ExpressionConstants.It, HardCodedTestModel.GetPersonTypeReference(), HardCodedTestModel.GetPeopleSet());
            this.bindingState = new BindingState(configuration) { ImplicitRangeVariable = implicitRangeVariable };
            this.bindingState.RangeVariables.Push(new BindingState(configuration) { ImplicitRangeVariable = implicitRangeVariable }.ImplicitRangeVariable);
        }

        [TestMethod]
        public void BindOrderByWithEmptyListOfOrderByTokensReturnsNull()
        {
            var tokens = new List<OrderByToken>();
            var result = orderbyBinder.BindOrderBy(this.bindingState, tokens);
            result.Should().BeNull();
        }

        [TestMethod]
        public void BindOrderByGetsDirectionFromToken()
        {
            var tokens = new List<OrderByToken>
                {
                    new OrderByToken(new LiteralToken("prop0"), OrderByDirection.Ascending)
                };

            var result = orderbyBinder.BindOrderBy(this.bindingState, tokens);
            result.Direction.Should().Be(OrderByDirection.Ascending);

            tokens = new List<OrderByToken>
                {
                    new OrderByToken(new LiteralToken("prop0"), OrderByDirection.Descending)
                };

            result = orderbyBinder.BindOrderBy(this.bindingState, tokens);
            result.Direction.Should().Be(OrderByDirection.Descending);
        }

        [TestMethod]
        public void BindOrderByShouldReturnAnOrderByNodeForEachToken()
        {
            var metadataBinder = new MetadataBinder(this.bindingState);
            this.orderbyBinder = new OrderByBinder(metadataBinder.Bind);
            var tokens = GetDummyOrderbyTokenList(7);
            var result = orderbyBinder.BindOrderBy(this.bindingState, tokens);
            result.ThenBy.ThenBy.ThenBy.ThenBy.ThenBy.ThenBy.ThenBy.Should().BeNull();
        }

        [TestMethod]
        public void BindOrderByShouldOrderByInEasyToConsumeOrder()
        {
            var metadataBinder = new MetadataBinder(this.bindingState);
            this.orderbyBinder = new OrderByBinder(metadataBinder.Bind);
            var tokens = GetDummyOrderbyTokenList(2); // $orderby=prop0, prop1
            var result = orderbyBinder.BindOrderBy(this.bindingState, tokens);
            result.Expression.ShouldBeConstantQueryNode("prop0");
            result.ThenBy.Expression.ShouldBeConstantQueryNode("prop1");
        }

        private IEnumerable<OrderByToken> GetDummyOrderbyTokenList(int numberOfTokens)
        {
            var list = new List<OrderByToken>();
            for (var i = 0; i < numberOfTokens; i++)
            {
                list.Add(new OrderByToken(new LiteralToken("prop" + i), OrderByDirection.Descending));
            }
            return list;
        }
    }
}
