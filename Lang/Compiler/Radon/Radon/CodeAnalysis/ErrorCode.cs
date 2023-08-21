namespace Radon.CodeAnalysis;

public enum ErrorCode
{
    // Internal Error
    InternalError = 0001,
    
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once UnusedMember.Global
    SYNTAX_ERROR = 1000, // This is just used to set the starting point for the syntax error codes.
    
    // Lexer Errors
    InvalidNumber,
    UnterminatedString,
    UnterminatedMultiLineComment,
    UnexpectedCharacter,
    
    // Parser Errors
    UnexpectedToken,
    ExpectedToken,
    UnfinishedDirective,
    IncludePathDoesNotExist,
    InvalidTypeDeclaration,
    InvalidTypeStart,
    InvalidIncludePath,
    DuplicateInclude,
    CircularInclude,
    ExpressionCannotBeStatement,
    InvalidTypeModifier,
    InvalidMethodModifier,
    InvalidFieldModifier,

    // ReSharper disable once InconsistentNaming
    // ReSharper disable once UnusedMember.Global
    BINDING_ERROR = 2000, // This is just used to set the starting point for the binding error codes.
    
    // Semantic Errors
    SymbolAlreadyDeclared,
    UnresolvedSymbol,
    MultipleProgramUnits,
    ConstructorNameMismatch,
    UndefinedType,
    UndefinedBinaryOperator,
    InvalidImportPathType,
    CannotInvokeExpression,
    NullLiteral,
    InvalidLiteralExpression,
    UndefinedMember,
    ThisExpressionOutsideOfMethod,
    ThisExpressionInStaticMethod,
    UndefinedUnaryOperator,
    CannotConvert,
    CannotConvertSourceType,
    CannotConvertImplicitly,
    CycleInStructLayout,
    EnumMemberMustHaveConstantValue,
    IncorrectNumberOfTypeArguments,
    IncorrectNumberOfArguments,
    UnresolvedMethod,
    AmbiguousMethodCall,
    CannotInstantiateNonStruct,
    CouldNotBindTemplateMethod,
    CannotInstantiateNonArray,
    IndexMustBeInteger,
    CannotIndexNonArray,
    ArrayMustHaveSize,
    CannotReturnExpressionFromVoidMethod,
    MustReturnExpressionFromNonVoidMethod,
    SignKeyMustBeString,
    SignValueMustBeStringBoolOrNumeric,
    NullConstantValue,
    CannotHaveBothPublicAndPrivateModifier,
    DuplicateModifier,
    CannotAccessNonPublicMember,
    InvalidBreakOrContinue,
    EntryModifierOnlyAllowedOnMethodsAndTypes,
    EntryModifierMustBeAppliedToParentStruct,
    MultipleEntryTypes,
    MultipleEntryMethods,
}