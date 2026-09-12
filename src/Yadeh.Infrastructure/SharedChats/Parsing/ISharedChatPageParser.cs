using System.Diagnostics.CodeAnalysis;
using Yadeh.Application.SharedChats.Models;

namespace Yadeh.Infrastructure.SharedChats.Parsing;

internal interface ISharedChatPageParser
{
    bool TryParse(
        string html,
        [NotNullWhen(true)] out SharedChatSnapshot? snapshot);
}