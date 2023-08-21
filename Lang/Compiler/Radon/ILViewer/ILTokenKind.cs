namespace ILViewer;

// ReSharper disable once InconsistentNaming
internal enum ILTokenKind
{
    TypeIdentifier,
    VariableIdentifier,
    MethodIdentifier,
    FieldIdentifier,
    EnumMemberIdentifier,
    Keyword,
    Label,
    OpCode,
    Punctuation,
    String,
    Number,
    UnknownValue,
    DirectiveKeyword,
    Comment,
    Trivia,
    EOF
}