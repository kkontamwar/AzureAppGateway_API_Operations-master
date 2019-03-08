﻿using AppGateWay_Operation_FrontEnd.API_Operation;
using AppGateWay_Operation_FrontEnd.Constants;
using AppGateWay_Operation_FrontEnd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;

namespace AppGateWay_Operation_FrontEnd.Controllers
{
	public class GatewayController : Controller
	{
		private readonly string _apiHostedUrl = string.Empty;
		private readonly IOptions<ApiOperations.CustomSettings> _settings;
		private List<Resultcs> _resultset;


		public GatewayController(IOptions<ApiOperations.CustomSettings> settings)
		{
			_settings = settings;
			_apiHostedUrl = _settings.Value.azureLogUrl;
			_resultset = new List<Resultcs>();
		}

		public IActionResult Index()
		{
			return View();
		}

		public ActionResult AddProbe(UserInputModel inputData)
		{
			GatewayViewModel apiInputData = new GatewayViewModel();
			apiInputData.operation = Operations.CreateProbe.ToString();
			apiInputData.SubscriptionId = Constant.subscriptionId;
			apiInputData.ResourceGroupName = Constant.resourceGroupName;
			apiInputData.ApplicationGatewayName = Constant.applicationGatewayName;

			apiInputData.ProbeName = inputData.FIName + "CustomHealthProbe";
			apiInputData.Protocol = inputData.Protocol;

			apiInputData.Host = inputData.Host;
			apiInputData.Probepath = inputData.Probepath;

			apiInputData.HttpSettingsName = inputData.FIName + "AppGatewayHTTPSettings";
			apiInputData.HttpSettingsPort = inputData.Port;


			apiInputData.BckendPoolName = inputData.FIName + "AppGWBackendpool";
			apiInputData.BckendipAddress = inputData.Host;

			apiInputData.RuleName = inputData.FIName;
			apiInputData.RulePath = inputData.RulePath;

			apiInputData.brearerToken = inputData.BrearerToken;

			var result = ApiOperations.ExecutivePutApi(_apiHostedUrl, apiInputData);

			if (result == HttpStatusCode.OK)
			{
				_resultset.Add(new Resultcs() { Message = "Probe created successfully", Status = "Completed", State = true });
				AddAddHttpSettings(apiInputData);
			}
			else
			{
				_resultset.Add(new Resultcs() { Message = "Failed to create Probe", Status = "Error", State = false });
				_resultset.Add(new Resultcs() { Message = "Unable to create HttpSettings", Status = "Not Performed", State = false });
				_resultset.Add(new Resultcs() { Message = "Unable to create BackendAddressPool", Status = "Not Performed", State = false });
				_resultset.Add(new Resultcs() { Message = "Unable to create Rule", Status = "Not Performed", State = false });
			}

			TempData.Put("Result", _resultset);

			return RedirectToAction("SendResult");

		}


		public ActionResult SendResult()
		{
			var _resultset = TempData.Get<List<Resultcs>>("Result");
			if (_resultset == null)
			{
				_resultset = new List<Resultcs>();
			}

			return View(_resultset);
		}

		[HttpGet]
		public HttpStatusCode AddAddHttpSettings(GatewayViewModel inputData)
		{
			//var inputData = (GatewayViewModel)TempData["ipData"];

			if (TempData["myData"] != null)
			{
				_resultset = TempData["myData"] as List<Resultcs>;
			}

			TempData.Keep();

			inputData.operation = Operations.AddHTTPSettings.ToString();
			var result = ApiOperations.ExecutivePutApi(_apiHostedUrl, inputData);


			if (result == HttpStatusCode.OK)
			{
				_resultset.Add(new Resultcs() { Message = "HttpSettings created successfully", Status = "Completed", State = true });
				//RedirectToAction("AddBackendAddressPool", "Gateway");
				return AddBackendAddressPool(inputData);

			}
			else
			{
				_resultset.Add(new Resultcs() { Message = "Failed to create HttpSettings", Status = "Error", State = false });
				_resultset.Add(new Resultcs() { Message = "Unable to create BackendAddressPool", Status = "Not Performed", State = false });
				_resultset.Add(new Resultcs() { Message = "Unable to create Rule", Status = "Not Performed", State = false });
			}
			return HttpStatusCode.BadRequest;
		}

		[HttpGet]
		public HttpStatusCode AddBackendAddressPool(GatewayViewModel inputData)
		{
			//var inputData = (GatewayViewModel)TempData["ipData"];
			inputData.operation = Operations.BackendAddressPool.ToString();

			var result = ApiOperations.ExecutivePutApi(_apiHostedUrl, inputData);

			if (result == HttpStatusCode.OK)
			{
				_resultset.Add(new Resultcs() { Message = "BackendAddressPool created successfully", Status = "Completed", State = true });
				return AddRule(inputData);
			}
			else
			{
				_resultset.Add(new Resultcs() { Message = "Unable to create BackendAddressPool", Status = "Error", State = false });
				_resultset.Add(new Resultcs() { Message = "Unable to create Rule", Status = "Not Performed", State = false });
			}

			return HttpStatusCode.BadRequest;

		}

		[HttpGet]
		public HttpStatusCode AddRule(GatewayViewModel inputData)
		{
			//GatewayViewModel inputData = (GatewayViewModel)TempData["ipData"];
			inputData.operation = Operations.AddRule.ToString();

			var result = ApiOperations.ExecutivePutApi(_apiHostedUrl, inputData);

			if (result == HttpStatusCode.OK)
			{
				_resultset.Add(new Resultcs() { Message = "Rule created successfully", Status = "Completed", State = true });
				return HttpStatusCode.OK;
			}
			else
			{
				_resultset.Add(new Resultcs() { Message = "Unable to create Rule", Status = "Error", State = false });
			}

			return HttpStatusCode.BadRequest;
		}



	}


	public static class TempDataExtensions
	{
		public static void Put<T>(this ITempDataDictionary tempData, string key, T value) where T : class
		{
			tempData[key] = JsonConvert.SerializeObject(value);
		}

		public static T Get<T>(this ITempDataDictionary tempData, string key) where T : class
		{
			object o;
			tempData.TryGetValue(key, out o);
			return o == null ? null : JsonConvert.DeserializeObject<T>((string)o);
		}
	}

	//public static class SessionExtensions
	//{
	//    public static void Set<T>(this ISession session, string key, T value)
	//    {
	//        session.SetString(key, JsonConvert.SerializeObject(value));
	//    }

	//    public static T Get<T>(this ISession session, string key)
	//    {
	//        var value = session.GetString(key);
	//        return value == null ? default(T) :
	//            JsonConvert.DeserializeObject<T>(value);
	//    }
	//}
}
