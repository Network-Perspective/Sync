namespace NetworkPerspective.Sync.Application.Domain.Connectors
{
    public class CustomAttributeRelationship
    {
        public string PropName { get; set; }
        public string RelationshipName { get; set; }

        public CustomAttributeRelationship(string propName, string relationshipName)
        {
            PropName = propName;
            RelationshipName = relationshipName;
        }

        public override string ToString()
            => $"PropertyName: {PropName}, RelationshipName: {RelationshipName}";
    }
}