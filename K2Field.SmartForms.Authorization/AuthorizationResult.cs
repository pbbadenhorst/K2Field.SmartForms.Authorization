using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K2Field.SmartForms.Authorization
{
    /// <summary>
    /// Represents the result of an authorization request by a user for a defined K2 resource.
    /// </summary>
    public class AuthorizationResult
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationResult"/> class.
        /// </summary>
        /// <param name="securableName">The name of the K2 resource form whom access was requested.</param>
        /// <param name="securableType">The type of K2 resource.</param>
        /// <param name="authorized">A flag to indicate whether or not authorization was granted.</param>
        /// <param name="timestamp">A timestamp indicating the last time the authorization was evaluated.</param>
        public AuthorizationResult(string securableName, SecurableType securableType, bool authorized, DateTime timestamp)
        {
            this.SecurableName = securableName;
            this.SecurableType = securableType;
            this.Authorized = authorized;
            this.Timestamp = timestamp;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationResult"/> class.
        /// </summary>
        public AuthorizationResult()
        {

        }

        #endregion

        #region Properties

        public string SecurableName { get; set; }

        public SecurableType SecurableType { get; set; }

        public bool Authorized { get; set; }

        public DateTime Timestamp { get; set; }

        #endregion
    }
}
