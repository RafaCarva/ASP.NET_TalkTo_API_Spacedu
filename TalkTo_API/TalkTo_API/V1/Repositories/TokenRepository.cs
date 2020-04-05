using TalkTo_API.DataBase;
using TalkTo_API.V1.Models;
using TalkTo_API.V1.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TalkTo_API.V1.Repositories
{
    public class TokenRepository : ITokenRepository
    {
        private readonly TalkToContext _banco;

        public TokenRepository(TalkToContext banco)
        {
            _banco = banco;
        }
        public Token Obter(string refreshToken)
        {
            return _banco.Token.FirstOrDefault(a=>a.RefreshToken == refreshToken && a.Utilizado == false);
        }

        public void Atualizar(Token token)
        {
            _banco.Token.Update(token);
            _banco.SaveChanges();
        }

        public void Cadastrar(Token token)
        {
            _banco.Token.Add(token);
            _banco.SaveChanges();
        }

        
    }
}
