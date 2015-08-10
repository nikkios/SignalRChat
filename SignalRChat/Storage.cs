using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Configuration;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using SignalRChat.Models;

namespace SignalRChat
{
    public class Storage
    {
        private CloudBlobContainer Messages;
        private CloudBlobContainer Users;

        public Storage()
        {
            var accountName = WebConfigurationManager.AppSettings["AccountName"];
            var accountKey = WebConfigurationManager.AppSettings["AccountKey"];
            bool useHttps = WebConfigurationManager.AppSettings["DefaultEndpointsProtocol"].ToLower() == "https";

            StorageCredentials credentials = new StorageCredentials(accountName, accountKey);
            CloudStorageAccount storageAccount = new CloudStorageAccount(credentials, useHttps);

            // Create the blob client.
            var blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container.
            Messages = blobClient.GetContainerReference("messages");
            Users = blobClient.GetContainerReference("users");

            // Create the container if it doesn't already exist. 
            Messages.CreateIfNotExists();

            Users.CreateIfNotExists();

        }

        public List<Message> Load()
        {
            var downloadList = new ArrayList();
            List<Message> messages = new List<Message>();

            foreach (IListBlobItem item in Messages.ListBlobs(null, false))
            {
                downloadList.Add(item.Uri.Segments[2]);
            }

            foreach (string guid in downloadList)
            {
                CloudBlockBlob blockBlob = Messages.GetBlockBlobReference(guid);

                using (var memoryStream = new MemoryStream())
                {
                    blockBlob.DownloadToStream(memoryStream);
                    string txt = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
                    messages.Add(JsonConvert.DeserializeObject<Message>(txt));
                }
            }

            return messages.OrderBy(x => x.Time).ToList();
        }

        public void Save(Message msg)
        {
            var messageBlob = Messages.GetBlockBlobReference(msg.Guid.ToString());

            var json = JsonConvert.SerializeObject(msg);
            messageBlob.UploadText(json);
        }

        public User[] RemoveUser(string id)
        {
            try
            {
                CloudBlockBlob userBlob = Users.GetBlockBlobReference("userBlob");
                List<User> userList = null;

                using (var memoryStream = new MemoryStream())
                {
                    userBlob.DownloadToStream(memoryStream);
                    string txt = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
                    userList = JsonConvert.DeserializeObject<User[]>(txt).ToList();
                }

                userList.Remove(userList.FirstOrDefault(x => x.Id == id));

                var json = JsonConvert.SerializeObject(userList.ToArray());
                userBlob.UploadText(json);

                return userList.ToArray();
            }
            catch (Exception ex)
            {
                    //todo
            }

            return null;
        }

        public User[] AddUser(User user)
        {
            CloudBlockBlob userBlob = Users.GetBlockBlobReference("userBlob");

            List<User> userList = null;

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    userBlob.DownloadToStream(memoryStream);
                    string txt = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
                    userList = JsonConvert.DeserializeObject<User[]>(txt).ToList();
                }
            }
            catch (Exception)
            {
                //no blob found?

                userList = new List<User>();
                //throw;
            }

            
            userList.Add(user);

            var json = JsonConvert.SerializeObject(userList.ToArray());
            userBlob.UploadText(json);

            return userList.ToArray();
        }

        public void Clear()
        {
            var deleteList = new ArrayList();
            List<Message> messages = new List<Message>();

            foreach (IListBlobItem item in Messages.ListBlobs(null, false))
            {
                deleteList.Add(item.Uri.Segments[2]);
            }

            //delete messages
            foreach (string guid in deleteList)
            {
                var blob = Messages.GetBlockBlobReference(guid);
                blob.Delete();
            }

            //delete user blob
            var userBlob = Users.GetBlockBlobReference("userBlob");
            userBlob.Delete();

        }

    }
}