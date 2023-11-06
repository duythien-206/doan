﻿using dj_actionlayer.Business.Email;
using dj_actionlayer.DAO;
using dj_webdesigncore.AuthModel;
using dj_webdesigncore.Business.Auth;
using dj_webdesigncore.DTOs;
using dj_webdesigncore.DTOs.Lobby;
using dj_webdesigncore.Entities.BusinessEntity;
using dj_webdesigncore.Entities.CourseEntity;
using dj_webdesigncore.Entities.UserEntity;
using dj_webdesigncore.Enums.ApiEnums;
using dj_webdesigncore.Request.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace dj_actionlayer.Business.Auth
{
    public class AuthBusiness : BaseBusiness, IAuthBusiness
    {
        private readonly SendEmail _sendEmail = new SendEmail(Settings.emailConfig()[0], Settings.emailConfig()[1], Settings.emailConfig()[2], int.Parse(Settings.emailConfig()[3]));
        public async Task<LoginResponse<AuthDataRespon>> Login(RequestLogin request)
        {
            User user;
            if (request.UserName.Contains("@"))
            {
                user = _context.user.SingleOrDefault(x => x.UserPass == request.Password && x.UserEmail == request.UserName);
            }
            else
            {
                user = _context.user.SingleOrDefault(x => x.UserPass == request.Password && x.UserName == request.UserName);
            }
            if (user == null)
            {
                return new LoginResponse<AuthDataRespon>
                {
                    Success = dj_webdesigncore.Enums.AuthEnums.AuthStatusEnum.FAILED,
                    Message = "Invalid username/password"
                };
            }
            if (user.UserStatusId == 3)
            {
                return new LoginResponse<AuthDataRespon>
                {
                    Success = dj_webdesigncore.Enums.AuthEnums.AuthStatusEnum.UNACTIVEUSER,
                    Message = "Authenticate success",
                    Data = new AuthDataRespon
                    {
                        email = user.UserEmail,
                    }
                };
            }
            if (user.UserStatusId == 2)
            {
                return new LoginResponse<AuthDataRespon>
                {
                    Success = dj_webdesigncore.Enums.AuthEnums.AuthStatusEnum.LOCKFOREVER,
                    Message = "Authenticate success",
                    Data = new AuthDataRespon
                    {
                        email = user.UserEmail,
                    }
                };
            }
            if (user.UserStatusId == 4)
            {
                DateTime now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                DateTime a = new DateTime(user.UnlockTime.Value.Year, user.UnlockTime.Value.Month, user.UnlockTime.Value.Day);
                if (now > a)
                {
                    user.IsLock = false;
                    user.UserStatusId = 1;
                    await _context.SaveChangesAsync();
                    return new LoginResponse<AuthDataRespon>
                    {
                        Success = dj_webdesigncore.Enums.AuthEnums.AuthStatusEnum.SUCCESS,
                        Message = "Authenticate success",
                        Data = new AuthDataRespon
                        {
                            id = user.Id,
                            avatar = user.UserAvatarData40x40,
                            nickName = "Chiến thần Front End",
                            email = user.UserEmail,
                            name = user.UserFisrtName + " " + user.UserLastName,
                            Token = await GenToken(user),
                            role = (int)user.UserRoleId,
                            kyc = (bool)user.IsKYC
                        }
                    };
                }
                return new LoginResponse<AuthDataRespon>
                {
                    Success = dj_webdesigncore.Enums.AuthEnums.AuthStatusEnum.LOCKED,
                    Message = "Tài khoản của bạn bị khóa đến " + user.UnlockTime.Value.Day + "/" + user.UnlockTime.Value.Month + "/" + user.UnlockTime.Value.Year + " do vi phạm nguyên tắc cộng đồng! Mọi thắc mắc vui long liên hệ Quản trị viên.",
                    Data = new AuthDataRespon
                    {
                        email = user.UserEmail,
                    }
                };
            }
            return new LoginResponse<AuthDataRespon>
            {
                Success = dj_webdesigncore.Enums.AuthEnums.AuthStatusEnum.SUCCESS,
                Message = "Authenticate success",
                Data = new AuthDataRespon
                {
                    id = user.Id,
                    avatar = user.UserAvatarData40x40,
                    nickName = "Chiến thần Front End",
                    email = user.UserEmail,
                    name = user.UserFisrtName + " " + user.UserLastName,
                    Token = await GenToken(user),
                    role = (int)user.UserRoleId,
                    kyc = (bool)user.IsKYC
                }
            };
        }
        public async Task<LoginResponse<TokenModel>> RenewToken(TokenModel model)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var secretKeyBytes = Encoding.UTF8.GetBytes(Settings.SecretKey());
            var tokenValidateParam = new TokenValidationParameters
            {
                //tự cấp token
                ValidateIssuer = false,
                ValidateAudience = false,

                //ký vào token
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),

                ClockSkew = TimeSpan.Zero,

                ValidateLifetime = false //ko kiểm tra token hết hạn
            };
            try
            {
                //check 1: AccessToken valid format
                var tokenInVerification = jwtTokenHandler.ValidateToken(model.AccessToken, tokenValidateParam, out var validatedToken);

                //check 2: Check alg
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase);
                    if (!result)//false
                    {
                        return new LoginResponse<TokenModel>
                        {
                            Success = dj_webdesigncore.Enums.AuthEnums.AuthStatusEnum.FAILED,
                            Message = "Invalid token"
                        };
                    }
                }

                //check 3: Check accessToken expire?
                var utcExpireDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                var expireDate = await ConvertUnixTimeToDateTime(utcExpireDate);
                if (expireDate > DateTime.UtcNow)
                {
                    return new LoginResponse<TokenModel>
                    {
                        Success = dj_webdesigncore.Enums.AuthEnums.AuthStatusEnum.FAILED,
                        Message = "Access token has not yet expired"
                    };
                }

                //check 4: Check refreshtoken exist in DB
                var storedToken = _context.refresh_token.FirstOrDefault(x => x.Token == model.RefreshToken);
                if (storedToken == null)
                {
                    return new LoginResponse<TokenModel>
                    {
                        Success = dj_webdesigncore.Enums.AuthEnums.AuthStatusEnum.FAILED,
                        Message = "Refresh token does not exist"
                    };
                }

                //check 5: check refreshToken is used/revoked?
                if (storedToken.IsUsed)
                {
                    return new LoginResponse<TokenModel>
                    {
                        Success = dj_webdesigncore.Enums.AuthEnums.AuthStatusEnum.FAILED,
                        Message = "Refresh token has been used"
                    };
                }
                if (storedToken.IsRevoked)
                {
                    return new LoginResponse<TokenModel>
                    {
                        Success = dj_webdesigncore.Enums.AuthEnums.AuthStatusEnum.FAILED,
                        Message = "Refresh token has been revoked"
                    };
                }

                //check 6: AccessToken id == JwtId in RefreshToken
                var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                if (storedToken.JwtId != jti)
                {
                    return new LoginResponse<TokenModel>
                    {
                        Success = dj_webdesigncore.Enums.AuthEnums.AuthStatusEnum.FAILED,
                        Message = "Token doesn't match"
                    };
                }

                //Update token is used
                storedToken.IsRevoked = true;
                storedToken.IsUsed = true;
                _context.Update(storedToken);
                _context.SaveChanges();

                //create new token
                var user = _context.user.SingleOrDefault(nd => nd.Id == storedToken.UserId);
                var token = await GenToken(user);

                return new LoginResponse<TokenModel>
                {
                    Success = dj_webdesigncore.Enums.AuthEnums.AuthStatusEnum.SUCCESS,
                    Message = "Renew token success",
                    Data = token
                };
            }
            catch (Exception ex)
            {
                return new LoginResponse<TokenModel>
                {
                    Success = dj_webdesigncore.Enums.AuthEnums.AuthStatusEnum.FAILED,
                    Message = "Something went wrong"
                };
            }
        }
        public async Task<DateTime> ConvertUnixTimeToDateTime(long utcExpireDate)
        {
            var dateTimeInterval = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dateTimeInterval.AddSeconds(utcExpireDate).ToUniversalTime();
            return dateTimeInterval;
        }
        public async Task<TokenModel> GenToken(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var secretKeyBytes = Encoding.UTF8.GetBytes(Settings.SecretKey());

            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim("Id", user.Id.ToString()),
                    new Claim(ClaimTypes.Role, _context.user_role.Find(user.UserRoleId).UserRoleCode),
                             new Claim("LoginTime", DateTime.Now.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    

                    //roles
                }),
                // Thời gian token có hiệu lực
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescription);
            var accessToken = jwtTokenHandler.WriteToken(token);
            var refreshToken = await GenerateRefreshToken();

            //Lưu database
            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                JwtId = token.Id,
                UserId = user.Id,
                Token = refreshToken,
                IsUsed = false,
                IsRevoked = false,
                IssuedAt = DateTime.Now,
                // Thời gian refresh token có hiệu lực
                ExpiredAt = DateTime.Now.AddDays(1)
            };
            await _context.AddAsync(refreshTokenEntity);
            await _context.SaveChangesAsync();
            return new TokenModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
        public async Task<string> GenerateRefreshToken()
        {
            var random = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(random);
                return Convert.ToBase64String(random);
            }
        }

        public async Task<ResponData<NewAccount>> SignIn(SignInAccount newAccount)
        {
            ResponData<NewAccount> result = new ResponData<NewAccount>();
            try
            {
                NewAccount acc = new NewAccount();
                if (await _context.user.Where(x => x.UserName == newAccount.userName).FirstOrDefaultAsync() != null)
                {
                    acc.Status = dj_webdesigncore.Enums.AuthEnums.SignInEnum.USEREXIST;
                    acc.Detail = "Tài khoản đã tồn tại!";
                    result.Data = acc;
                    result.Messenger = "Lấy dữ liệu thành công!";
                    return result;
                }
                if (await _context.user.Where(x => x.UserEmail == newAccount.email).FirstOrDefaultAsync() != null)
                {
                    acc.Status = dj_webdesigncore.Enums.AuthEnums.SignInEnum.EMAILEXIST;
                    acc.Detail = "Email đã sử dụng!";
                    result.Data = acc;
                    result.Messenger = "Lấy dữ liệu thành công!";
                    return result;
                }
                if (await _context.user.Where(x => x.NumberPhone == newAccount.sdt).FirstOrDefaultAsync() != null)
                {
                    acc.Status = dj_webdesigncore.Enums.AuthEnums.SignInEnum.PHONEEXIST;
                    acc.Detail = "SDT đã sử dụng!";
                    result.Data = acc;
                    result.Messenger = "Lấy dữ liệu thành công!";
                    return result;
                }
                acc.Detail = "Thành công chờ xác nhận Email!";
                acc.Status = dj_webdesigncore.Enums.AuthEnums.SignInEnum.SECCESSFULLY;
                User user = new User();
                user.NumberPhone = newAccount.sdt;
                user.UserName = newAccount.userName.ToLower();
                user.UserPass = newAccount.password;
                user.UserEmail = newAccount.email.ToLower();
                user.UserFisrtName = "Nhân Tố ";
                user.UserLastName = "Bí Ẩn";
                user.UserRoleId = 3;
                user.GenderId = 4;
                user.UserStatusId = 3;
                user.CreateAccount = DateTime.Now;
                user.IsKYC = false;
                user.VioCount = 0;
                user.UserAvatarData40x40 = "https://res.cloudinary.com/dushydq6r/image/upload/v1686398105/xyz-abc_1286085158_6/10/vhwjtmzsygp0f2t1o8wc.jpg";
                await _context.user.AddAsync(user);
                await _context.SaveChangesAsync();
                ConfirmEmail confirmEmail = new ConfirmEmail();
                confirmEmail.IsConfirm = false;
                confirmEmail.UserId = user.Id;
                confirmEmail.RequiredDateTime = DateTime.Now;
                confirmEmail.ExpiredDateTime = DateTime.Now.AddDays(1);
                Random rand = new Random();
                string code = "DJ" + rand.Next(10000000, 99999999);
                while (_context.confirm_email.Any(x => x.Code.Equals(code)))
                {
                    code = "DJ" + rand.Next(10000000, 99999999);
                }
                confirmEmail.Code = code;
                await _context.confirm_email.AddAsync(confirmEmail);
                await _context.SaveChangesAsync();
                _sendEmail.SendConfirmCreateAccount(newAccount.email, confirmEmail.Code);
                acc.Email = newAccount.email;
                result.Data = acc;
                result.Status = dj_webdesigncore.Enums.ApiEnums.ActionStatus.WAITEMAILCOMFIRM;
                result.Messenger = "Lấy dữ liệu thành công!";
                return result;
            }
            catch (Exception ex)
            {
                result.Status = dj_webdesigncore.Enums.ApiEnums.ActionStatus.FAILED;
                result.Messenger = "Lấy dữ liệu thất bại! Exception: " + ex.Message;
                return result;
            }
        }

        public async Task<LoginResponse<AuthDataRespon>> ConfirmAccount(string code)
        {
            LoginResponse<AuthDataRespon> result = new LoginResponse<AuthDataRespon>();

            ConfirmEmail confirmEmail = _context.confirm_email.Where(x => x.Code.Equals(code)).SingleOrDefault();
            if (confirmEmail == null)
            {
                result.Success = dj_webdesigncore.Enums.AuthEnums.AuthStatusEnum.FAILED;
                result.Message = "Xác nhận thất bại!";
                result.Data = null;
                return result;
            }
            if (confirmEmail.ExpiredDateTime < DateTime.Now)
            {
                result.Success = dj_webdesigncore.Enums.AuthEnums.AuthStatusEnum.EXPIRED;
                result.Message = "Mã xác nhận đã hết hạn!";
                result.Data = null;
                return result;
            }
            User user = _context.user.Find(confirmEmail.UserId);
            user.UserStatusId = 1;
            _context.confirm_email.Remove(confirmEmail);
            await _context.SaveChangesAsync();
            result.Message = "Xác nhận đăng ký thành công tài khoản đã có thể sử dụng!";
            result.Data = new AuthDataRespon
            {
                id = user.Id,
                avatar = user.UserAvatarData40x40,
                nickName = "Chiến thần Front End",
                name = user.UserFisrtName + " " + user.UserLastName,
                Token = await GenToken(user),
                role = (int)user.UserRoleId
            };
            Notification notification = new Notification();
            notification.SystemNotification = true;
            notification.Content = "Đăng ký tài khoản thành công. Mọi thắc mắc và góp ý vui lòng gửi về do.quanganh99zz@gmail.com hoặc 0968491797.";
            notification.UserId = user.Id;
            notification.Create = DateTime.Now;
            notification.IsSeen = false;
            notification.Link = null;
            await _context.AddAsync(notification);
            await _context.SaveChangesAsync();
            return result;
        }

        public async Task<ResponData<ActionStatus>> ForgetPass(ForgetPassRequest forgetPassRequest)
        {
            ResponData<ActionStatus> result = new ResponData<ActionStatus>();
            User user = await _context.user.Where(x => x.UserEmail.ToLower().Equals(forgetPassRequest.Email.ToLower())).FirstOrDefaultAsync();
            if (user == null)
            {
                result.Data = ActionStatus.NOTFOUND;
                result.Status = ActionStatus.SECCESSFULLY;
                result.Messenger = "Lấy dữ liệu thành công!";
                return result;
            }
            var listConfirm = _context.confirm_email.Where(x => x.UserId == user.Id).ToList();
            _context.confirm_email.RemoveRange(listConfirm);
            await _context.SaveChangesAsync();
            ConfirmEmail confirmEmail = new ConfirmEmail();
            confirmEmail.IsConfirm = false;
            confirmEmail.UserId = user.Id;
            confirmEmail.RequiredDateTime = DateTime.Now;
            confirmEmail.ExpiredDateTime = DateTime.Now.AddDays(1);
            Random rand = new Random();
            string code = "DJ" + rand.Next(10000000, 99999999);
            while (_context.confirm_email.Any(x => x.Code.Equals(code)))
            {
                code = "DJ" + rand.Next(10000000, 99999999);
            }
            confirmEmail.Code = code;
            await _context.confirm_email.AddAsync(confirmEmail);
            await _context.SaveChangesAsync();
            _sendEmail.SendForgetPass(forgetPassRequest.Email.ToLower(), confirmEmail.Code);
            result.Data = ActionStatus.SECCESSFULLY;
            result.Status = ActionStatus.SECCESSFULLY;
            result.Messenger = "Lấy dữ liệu thành công!";
            return result;
        }

        public async Task<ResponData<ActionStatus>> ConfirmCodeForgetPass(string code)
        {
            ResponData<ActionStatus> result = new ResponData<ActionStatus>();
            ConfirmEmail confirmEmail1 = await _context.confirm_email.Where(x => x.Code.Equals(code)).FirstOrDefaultAsync();
            if (confirmEmail1 == null)
            {
                result.Status = ActionStatus.SECCESSFULLY;
                result.Data = ActionStatus.NOTFOUND;
                result.Messenger = "Lấy dữ liệu thành công!";
                return result;
            }
            if (confirmEmail1.ExpiredDateTime < DateTime.Now)
            {
                result.Status = ActionStatus.SECCESSFULLY;
                result.Data = ActionStatus.FAILED;
                result.Messenger = "Lấy dữ liệu thành công!";
                return result;
            }
            Notification notification = new Notification();
            notification.SystemNotification = true;
            notification.Content = "Bạn đã lấy lại mật khẩu thành công nhớ bảo mật mật khẩu mới nha. mãi yêu :3";
            notification.UserId = confirmEmail1.UserId;
            notification.Create = DateTime.Now;
            notification.IsSeen = false;
            notification.Link = null;
            await _context.AddAsync(notification);
            await _context.SaveChangesAsync();
            result.Data = ActionStatus.SECCESSFULLY;
            result.Status = ActionStatus.SECCESSFULLY;
            result.Messenger = "Lấy dữ liệu thành công!";
            return result;
        }
        public async Task<LoginResponse<AuthDataRespon>> ConfirmNewPass(ConfirmNewPass confirmNewPass)
        {
            LoginResponse<AuthDataRespon> result = new LoginResponse<AuthDataRespon>();

            ConfirmEmail confirmEmail = await _context.confirm_email.Where(x => x.Code.Equals(confirmNewPass.Code)).FirstOrDefaultAsync();
            if (confirmEmail == null)
            {
                result.Success = dj_webdesigncore.Enums.AuthEnums.AuthStatusEnum.FAILED;
                result.Message = "Xác nhận thất bại!";
                result.Data = null;
                return result;
            }
            if (confirmEmail.ExpiredDateTime < DateTime.Now)
            {
                result.Success = dj_webdesigncore.Enums.AuthEnums.AuthStatusEnum.EXPIRED;
                result.Message = "Mã xác nhận đã hết hạn!";
                result.Data = null;
                return result;
            }
            User user = _context.user.Find(confirmEmail.UserId);
            user.UserPass = confirmNewPass.NewPass;
            _context.confirm_email.Remove(confirmEmail);
            await _context.SaveChangesAsync();
            result.Message = "Xác nhận đổi mật khẩu thành công!";
            result.Data = new AuthDataRespon
            {
                id = user.Id,
                avatar = user.UserAvatarData40x40,
                nickName = "Chiến thần Front End",
                name = user.UserFisrtName + " " + user.UserLastName,
                Token = await GenToken(user),
                role = (int)user.UserRoleId
            };
            return result;
        }
    }
}
