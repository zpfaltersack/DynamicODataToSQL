namespace DynamicODataToSQL;
using System.Collections.Generic;

using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

using SqlKata.Compilers;

public class ColumnNameResolver(Compiler compiler, string tableName)
{
    public HashSet<IEdmNavigationProperty> NavigationProperties { get; } = [];

    /// <param name="wrap">if true returned value will be wrapped in opening and closing column identifier</param>
    public string GetColumnName(QueryNode node, bool wrap = false)
    {
        var table = tableName;
        var column = string.Empty;
        if (node.Kind == QueryNodeKind.Convert)
        {
            node = (node as ConvertNode).Source;
        }

        if (node.Kind == QueryNodeKind.SingleValuePropertyAccess)
        {
            // I would like to get rid of the is/as casts. Are there some first class
            // interface members I can use?
            // Additionally, is this the place where I can maybe dereference the entity set linked
            // to by the lookup property?
            if (node is SingleValuePropertyAccessNode singleValueProperty
                && singleValueProperty.Source is SingleNavigationNode navigationNode)
            {
                column = (node as SingleValuePropertyAccessNode).Property.Name.Trim();
                table = navigationNode.NavigationProperty.Name;

                NavigationProperties.Add(navigationNode.NavigationProperty);
            }
            else
            {
                column = (node as SingleValuePropertyAccessNode).Property.Name.Trim();
            }
        }

        if (node.Kind == QueryNodeKind.SingleValueOpenPropertyAccess)
        {
            column = (node as SingleValueOpenPropertyAccessNode).Name.Trim();
        }

        if (wrap)
        {
            table = compiler.WrapValue(table);
            column = compiler.WrapValue(column);
        }

        return $"{table}.{column}".Replace(ODataToSqlConverter.SPACESIGNREPLACEMENT, " ");
    }
}
