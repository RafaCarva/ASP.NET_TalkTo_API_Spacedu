﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using TalkTo_API.V1.Models;
using System.Text;
using System.Security.Claims;
using TalkTo_API.V1.Repositories.Contracts;

namespace TalkTo_API.V1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenRepository _tokenRepository;

        public UsuarioController(
            IUsuarioRepository usuarioRepository,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ITokenRepository tokenRepository
            )
        {
            _usuarioRepository = usuarioRepository;
            _signInManager = signInManager;
            _userManager = userManager;
            _tokenRepository = tokenRepository;
        }

        [HttpPost("login")]
        public ActionResult Login([FromBody]UsuarioDTO usuarioDTO)
        {
            ModelState.Remove("Nome");
            ModelState.Remove("ConfirmacaoSenha");

            if (ModelState.IsValid)
            {
                ApplicationUser usuario = _usuarioRepository.Obter(usuarioDTO.Email, usuarioDTO.Senha);
                if (usuario != null)
                {
                    // Login no Identity
                    // _signInManager.SignInAsync(usuario, false);

                    // retornar o token JWT
                    return GerarToken(usuario);
                }
                else
                {
                    return NotFound("Usuário não localizado!");
                }
            }
            else
            {
                return UnprocessableEntity(ModelState);
            }
        }

        private ActionResult GerarToken(ApplicationUser usuario)
        {
            var token = BuildToken(usuario);

            // Salvar o token no banco
            var tokenModel = new Token()
            {
                RefreshToken = token.RefreshToken,
                ExpirationToken = token.Expiration,
                ExpirationRefreshToken = token.ExpirationRefreshToken,
                Usuario = usuario,
                Criado = DateTime.Now,
                Utilizado = false
            };
            _tokenRepository.Cadastrar(tokenModel);
            return Ok(token);
        }

        private TokenDTO BuildToken(ApplicationUser usuario)
        {
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("chave-api-jwt-minhas-tarefas")); // Recomendado -> appsettings.json
            var sign = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var exp = DateTime.UtcNow.AddHours(1);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: exp,
                signingCredentials: sign
            );

            var refreshToken = Guid.NewGuid().ToString().Replace("-", "");
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            var expRefreshToken = DateTime.UtcNow.AddHours(2);
            var tokenDTO = new TokenDTO
            {
                Token = tokenString,
                Expiration = exp,
                RefreshToken = refreshToken,
                ExpirationRefreshToken = expRefreshToken
            };

            return tokenDTO;
        }

        [HttpPost("renovar")]
        public ActionResult Renovar([FromBody]TokenDTO tokenDTO)
        {
            var refreshTokenDB = _tokenRepository.Obter(tokenDTO.RefreshToken);
            if (refreshTokenDB == null)
            {
                return NotFound();
            }
            // RefreshToken antigo - Atualizar - Desativar esse refreshToken
            refreshTokenDB.Atualizado = DateTime.Now;
            refreshTokenDB.Utilizado = true;
            _tokenRepository.Atualizar(refreshTokenDB);

            // Gerar o novo token/Refresh - salvar 
            var usuario = _usuarioRepository.Obter(refreshTokenDB.UsuarioId);
            return GerarToken(usuario);

        }

        [HttpPost("")]
        public ActionResult Cadastrar([FromBody]UsuarioDTO usuarioDTO)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser usuario = new ApplicationUser();
                usuario.FullName = usuarioDTO.Nome;
                usuario.UserName = usuarioDTO.Email;
                usuario.Email = usuarioDTO.Email;
                var resultado = _userManager.CreateAsync(usuario, usuarioDTO.Senha).Result;

                if (!resultado.Succeeded)
                {
                    List<string> erros = new List<string>();
                    foreach (var erro in resultado.Errors)
                    {
                        erros.Add(erro.Description);
                    }
                    return UnprocessableEntity(erros);
                }
                else
                {
                    return Ok(usuario);
                }
            }
            else
            {
                return UnprocessableEntity(ModelState);
            }
        }
    }
}