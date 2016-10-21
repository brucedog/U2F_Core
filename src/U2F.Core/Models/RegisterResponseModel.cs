using System;
using System.Linq;

namespace U2F.Core.Models
{
    public class RegisterResponseModel : BaseModel
    {
        private readonly ClientData _clientDataRef;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterResponseModel"/> class.
        /// </summary>
        /// <param name="registrationData">The registration data.</param>
        /// <param name="clientData">The client data.</param>
        public RegisterResponseModel(String registrationData, String clientData)
        {
            if (String.IsNullOrWhiteSpace(registrationData) || String.IsNullOrWhiteSpace(clientData))
                throw new ArgumentException("Invalid argument(s) were being passed.");

            RegistrationData = registrationData;
            ClientData = clientData;
            _clientDataRef = new ClientData(ClientData);
        }

        /// <summary>
        /// Gets the registration data.
        /// </summary>
        /// <value>
        /// The registration data.
        /// </value>
        public String RegistrationData { get; private set; }

        /// <summary>
        /// Gets the Client data.
        /// </summary>
        /// <value>
        /// The Client data.
        /// </value>
        public String ClientData { get; private set; }

        public ClientData GetClientData()
        {
            return _clientDataRef;
        }

        public String GetRequestId()
        {
            return GetClientData().Challenge;
        }

        public override int GetHashCode()
        {
            int hash = RegistrationData.Sum(c => c + 31);
            hash += ClientData.Sum(c => c + 31);

            return hash;
        }

        public override bool Equals(Object obj)
        {
            if (!(obj is RegisterResponseModel))
                return false;
            if (this == obj)
                return true;
            if (GetType() != obj.GetType())
                return false;
            RegisterResponseModel other = (RegisterResponseModel)obj;
            if (ClientData == null)
            {
                if (other.ClientData != null)
                    return false;
            }
            else if (!ClientData.Equals(other.ClientData))
                return false;
            if (RegistrationData == null)
            {
                if (other.RegistrationData != null)
                    return false;
            }
            else if (!RegistrationData.Equals(other.RegistrationData))
                return false;
            return true;
        }
    }
}