﻿using System.Collections.Immutable;

namespace Shore.CodeAnalysis.Binding
{
    internal abstract class BoundTreeRewriter
    {        
        public virtual BoundStatement RewriteStatement(BoundStatement node)
        {
            return node.Kind switch
            {
                BoundNodeKind.BlockStatement => RewriteBlockStatement((BoundBlockStatement)node),
                BoundNodeKind.VariableDeclaration => RewriteVariableDeclaration((BoundVariableDeclaration)node),
                BoundNodeKind.ArrayDeclaration => RewriteArrayDeclaration((BoundArrayDeclaration)node),
                BoundNodeKind.ListDeclaration => RewriteListDeclaration((BoundListDeclaration)node),
                BoundNodeKind.DictDeclaration => RewriteDictDeclaration((BoundDictDeclaration)node),
                BoundNodeKind.IfStatement => RewriteIfStatement((BoundIfStatement)node),
                BoundNodeKind.WhileStatement => RewriteWhileStatement((BoundWhileStatement)node),
                BoundNodeKind.ForStatement => RewriteForStatement((BoundForStatement)node),
                BoundNodeKind.LabelStatement => RewriteLabelStatement((BoundLabelStatement)node),
                BoundNodeKind.GotoStatement => RewriteGotoStatement((BoundGotoStatement)node),
                BoundNodeKind.ConditionalGotoStatement => RewriteConditionalGotoStatement((BoundConditionalGotoStatement)node),
                BoundNodeKind.ReturnStatement => RewriteReturnStatement((BoundReturnStatement)node),
                BoundNodeKind.ExpressionStatement => RewriteExpressionStatement((BoundExpressionStatement)node),
                _ => throw new Exception($"Unexpected Node: {node.Kind}")
            };
        }

        protected virtual BoundStatement RewriteBlockStatement(BoundBlockStatement node)
        {
            ImmutableArray<BoundStatement>.Builder? builder = null;

            for (var i = 0; i < node.Statements.Length; i++)
            {
                var oldStatement = node.Statements[i];
                var newStatement = RewriteStatement(oldStatement);
                if (newStatement != oldStatement)
                {
                    if (builder is null)
                    {
                        builder = ImmutableArray.CreateBuilder<BoundStatement>(node.Statements.Length);
                        for (var j = 0; j < i; j++) builder.Add(node.Statements[j]);
                    }                    
                }

                if (builder is not null) builder.Add(newStatement);
            }

            if (builder is null) return node;

            return new BoundBlockStatement(builder.MoveToImmutable());
        }

        protected virtual BoundStatement RewriteVariableDeclaration(BoundVariableDeclaration node)
        {
            var initializer = RewriteExpression(node.Initializer);
            return initializer == node.Initializer ? node : new BoundVariableDeclaration(node.Variable, initializer);
        }

        protected virtual BoundStatement RewriteArrayDeclaration(BoundArrayDeclaration node) => node;
        
        protected virtual BoundStatement RewriteListDeclaration(BoundListDeclaration node) => node;
        protected virtual BoundStatement RewriteDictDeclaration(BoundDictDeclaration node) => node;
        
        protected virtual BoundStatement RewriteIfStatement(BoundIfStatement node)
        {
            var condition = RewriteExpression(node.Condition);
            var thenStatement = RewriteStatement(node.ThenStatement);
            var elseStatement = node.ElseStatement is null ? null : RewriteStatement(node.ElseStatement);
            if (condition == node.Condition && thenStatement == node.ThenStatement &&
                elseStatement == node.ElseStatement) return node;

            return new BoundIfStatement(condition, thenStatement, elseStatement);
        }

        protected virtual BoundStatement RewriteWhileStatement(BoundWhileStatement node)
        {
            var condition = RewriteExpression(node.Condition);
            var body = RewriteStatement(node.Body);
            if (condition == node.Condition && body == node.Body) return node;

            return new BoundWhileStatement(condition, body, node.BreakLabel, node.ContinueLabel);
        }

        protected virtual BoundStatement RewriteForStatement(BoundForStatement node)
        {
            var lowerBound = RewriteExpression(node.LowerBound);
            var upperBound = RewriteExpression(node.UpperBound);
            var body = RewriteStatement(node.Body);
            if (lowerBound == node.LowerBound && upperBound == node.UpperBound && body == node.Body) return node;

            return new BoundForStatement(node.Variable, lowerBound, upperBound, body, node.BreakLabel, node.ContinueLabel);
        }

        protected virtual BoundStatement RewriteLabelStatement(BoundLabelStatement node) => node;

        protected virtual BoundStatement RewriteGotoStatement(BoundGotoStatement node) => node;

        protected virtual BoundStatement RewriteConditionalGotoStatement(BoundConditionalGotoStatement node)
        {
            var condition = RewriteExpression(node.Condition);
            if (condition == node.Condition) return node;

            return new BoundConditionalGotoStatement(node.BoundLabel, condition, node.JumpIfTrue);
        }

        protected virtual BoundStatement RewriteReturnStatement(BoundReturnStatement node)
        {
            var expression = node.Expression == null ? null : RewriteExpression(node.Expression);
            if (expression == node.Expression)
                return node;

            return new BoundReturnStatement(expression);
        }
        
        protected virtual BoundStatement RewriteExpressionStatement(BoundExpressionStatement node)
        {
            var expression = RewriteExpression(node.Expression);
            if (expression == node.Expression) return node;

            return new BoundExpressionStatement(expression);
        }

        public virtual BoundExpression? RewriteExpression(BoundExpression? node)
        {
            return node.Kind switch
            {
                BoundNodeKind.NullExpression => RewriteNullExpression((BoundNullExpression)node),
                BoundNodeKind.LiteralExpression => RewriteLiteralExpression((BoundLiteralExpression)node),
                BoundNodeKind.VariableExpression => RewriteVariableExpression((BoundVariableExpression)node),
                BoundNodeKind.ArrayExpression => RewriteArrayExpression((BoundArrayExpression)node),
                BoundNodeKind.ListExpression => RewriteListExpression((BoundListExpression)node),
                BoundNodeKind.DictExpression => RewriteDictExpression((BoundDictExpression)node),
                BoundNodeKind.AssignmentExpression => RewriteAssignmentExpression((BoundAssignmentExpression)node),
                BoundNodeKind.ListAssignmentExpression => RewriteListAssignmentExpression((BoundListAssignmentExpression)node),
                BoundNodeKind.UnaryExpression => RewriteUnaryExpression((BoundUnaryExpression)node),
                BoundNodeKind.BinaryExpression => RewriteBinaryExpression((BoundBinaryExpression)node),
                BoundNodeKind.CallExpression => RewriteCallExpression((BoundCallExpression)node),
                BoundNodeKind.ConversionExpression => RewriteConversionExpression((BoundConversionExpression)node),
                _ => throw new Exception($"Unexpected Node: {node.Kind}")
            };
        }

        protected virtual BoundExpression? RewriteNullExpression(BoundNullExpression? node) => node;
        
        protected virtual BoundExpression? RewriteLiteralExpression(BoundLiteralExpression? node) => node;

        protected virtual BoundExpression? RewriteVariableExpression(BoundVariableExpression? node) => node;
        
        protected virtual BoundExpression? RewriteArrayExpression(BoundArrayExpression? node) => node;
        
        protected virtual BoundExpression? RewriteListExpression(BoundListExpression? node) => node;
        
        protected virtual BoundExpression? RewriteDictExpression(BoundDictExpression? node) => node;

        protected virtual BoundExpression? RewriteAssignmentExpression(BoundAssignmentExpression? node)
        {
            var expression = RewriteExpression(node.Expression);
            if (expression == node.Expression) return node;

            return new BoundAssignmentExpression(node.Variable, expression);
        }
        
        protected virtual BoundExpression? RewriteListAssignmentExpression(BoundListAssignmentExpression? node)
        {
            var expression = RewriteExpression(node.Expression);
            if (expression == node.Expression) return node;
            
            return new BoundListAssignmentExpression(node.Variable, expression, node.Accessor);
        }


        protected virtual BoundExpression? RewriteUnaryExpression(BoundUnaryExpression? node)
        {
            var operand = RewriteExpression(node.Operand);
            if (operand == node.Operand) return node;

            return new BoundUnaryExpression(node.Op, operand);
        }

        protected virtual BoundExpression? RewriteBinaryExpression(BoundBinaryExpression? node)
        {
            var left = RewriteExpression(node.Left);
            var right = RewriteExpression(node.Right);
            if (left == node.Left && right == node.Right) return node;

            return new BoundBinaryExpression(left, node.Op, right);
        }

        protected virtual BoundExpression? RewriteCallExpression(BoundCallExpression? node)
        {
            ImmutableArray<BoundExpression?>.Builder? builder = null;

            for (int i = 0; i < node.Arguments.Length; i++)
            {
                var oldArgument = node.Arguments[i];
                var newArgument = RewriteExpression(oldArgument);
                if (newArgument != oldArgument && builder is null)
                {
                    builder = ImmutableArray.CreateBuilder<BoundExpression>(node.Arguments.Length);
                    for (var j = 0; j < i; j++) builder.Add(node.Arguments[j]);
                }

                builder?.Add(newArgument);
            }

            return builder is null ? node : new BoundCallExpression(node.Function, builder.MoveToImmutable());
        }

        protected virtual BoundExpression? RewriteConversionExpression(BoundConversionExpression? node)
        {
            var expression = RewriteExpression(node.Expression);
            if (expression == node.Expression) return node;
            return new BoundConversionExpression(node.Type, expression);
        }
    }
}