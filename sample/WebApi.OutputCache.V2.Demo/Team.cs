﻿using System;

namespace WebApi.OutputCache.V2.Demo
{
    [Serializable]
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string League { get; set; }
    }
}