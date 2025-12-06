using System;
using UnityEngine;

namespace WezzelNL.Gemini
{
	[Serializable]
    public struct GeminiAccessToken : IEquatable<GeminiAccessToken> 
    {
        [field: SerializeField]public string AccessToken { get; private set; }

        private GeminiAccessToken(string accessToken)
        {
            AccessToken = accessToken;
        }

        public bool Equals(GeminiAccessToken other)
        {
            return AccessToken == other.AccessToken;
        }

        public override bool Equals(object obj)
        {
            return obj is GeminiAccessToken other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (AccessToken != null ? AccessToken.GetHashCode() : 0);
        }
        public static GeminiAccessToken Create(string apiKey) => new GeminiAccessToken(apiKey);
    }
}