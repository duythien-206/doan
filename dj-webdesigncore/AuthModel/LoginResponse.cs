﻿using dj_webdesigncore.Enums.AuthEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dj_webdesigncore.AuthModel
{
    public class LoginResponse<T>
    {
        public AuthStatusEnum Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}
