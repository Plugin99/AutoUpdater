﻿using GeneralUpdate.AspNetCore.Models;
using GeneralUpdate.Core.DTOs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeneralUpdate.AspNetCore.Services
{
    public class GeneralUpdateService : IUpdateService
    {
        /// <summary>
        /// Update validate.
        /// </summary>
        /// <param name="clientType">1:ClientApp 2:UpdateApp</param>
        /// <param name="clientVersion">The current version number of the client.</param>
        /// <param name="serverLastVersion">The latest version number of the server.</param>
        /// <param name="isForce">Do you need to force an update.</param>
        /// <param name="getUrlsAction">Each version update (Query the latest version information in the database according to the client version number).</param>
        /// <returns></returns>
        public async Task<string> UpdateValidateTaskAsync(int clientType, string clientVersion, string serverLastVersion, string clientAppkey, string appSecretKey,
            bool isForce, Func<int, string, Task<List<UpdateVersionDTO>>> getUrlsAction)
        {
            ParameterVerification(clientType, clientVersion, serverLastVersion, clientAppkey, appSecretKey, getUrlsAction);
            if (!clientAppkey.Equals(appSecretKey)) throw new Exception("App key does not exist or is incorrect !");
            Version clientLastVersion;
            var respDTO = new UpdateValidateRespDTO();
            try
            {
                if (!Version.TryParse(clientVersion, out clientLastVersion))
                {
                    respDTO.Message = $"{ RespMessage.RequestFailed } Wrong version number.";
                    respDTO.Code = ResponseStatus.Bad;
                    return null;
                }
                var lastVersion = new Version(serverLastVersion);
                if (clientLastVersion < lastVersion)
                {
                    respDTO.Body = new UpdateValidateDTO();
                    var body = respDTO.Body;
                    body.ClientType = clientType;
                    body.UpdateVersions = await getUrlsAction(clientType, clientVersion);
                    body.IsForcibly = isForce;
                    body.IsUpdate = true;
                    respDTO.Code = ResponseStatus.Success;
                    respDTO.Message = RespMessage.RequestSucceeded;
                }
                else
                {
                    respDTO.Body = new UpdateValidateDTO() { UpdateVersions = new List<UpdateVersionDTO>() , ClientType = clientType };
                    respDTO.Code = ResponseStatus.Success;
                    respDTO.Message = RespMessage.RequestNone;
                }
            }
            catch
            {
                respDTO.Message = RespMessage.ServerException;
                respDTO.Code = ResponseStatus.Internal;
            }
            return JsonConvert.SerializeObject(respDTO);
        }

        /// <summary>
        /// Return all updated version information.
        /// </summary>
        /// <param name="getUrlsAction">Each version update (Query the latest version information in the database according to the client version number).</param>
        /// <returns></returns>
        public async Task<string> UpdateVersionsTaskAsync(int clientType, string clientVersion, string clientAppkey, string appSecretKey, Func<int, string, Task<List<UpdateVersionDTO>>> getUrlsAction)
        {
            ParameterVerification(clientType, clientVersion, "skip", clientAppkey, appSecretKey, getUrlsAction);
            if (!clientAppkey.Equals(appSecretKey)) throw new Exception("App key does not exist or is incorrect !");
            var respDTO = new UpdateVersionsRespDTO();
            try
            {
                respDTO.Code = ResponseStatus.Success;
                respDTO.Message = RespMessage.RequestSucceeded;
                respDTO.Body = new UpdateVersionsDTO { UpdateVersions = await getUrlsAction(clientType, clientVersion) };
            }
            catch (Exception)
            {
                respDTO.Code = ResponseStatus.Internal;
                respDTO.Message = RespMessage.ServerException;
            }
            return JsonConvert.SerializeObject(respDTO);
        }

        private void ParameterVerification(int clientType, string clientVersion, string serverLastVersion, string clientAppkey,string appSecretKey, Func<int, string, Task<List<UpdateVersionDTO>>> getUrlsAction)
        {
            if (clientType <= 0) throw new Exception(@"'clientType' cannot be less than or equal to 0 !");
            if (string.IsNullOrWhiteSpace(clientVersion)) throw new ArgumentNullException(@"'clientVersion' cannot be null !");
            if (string.IsNullOrWhiteSpace(serverLastVersion)) throw new ArgumentNullException(@"'serverLastVersion' cannot be null !");
            if (getUrlsAction == null) throw new ArgumentNullException(@"'getUrlsAction' cannot be null!");
            if (string.IsNullOrEmpty(clientAppkey) || string.IsNullOrEmpty(appSecretKey)) throw new NullReferenceException("The APP key does not exist !");
        }
    }
}