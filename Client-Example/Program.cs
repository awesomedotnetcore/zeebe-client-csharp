﻿//
//    Copyright (c) 2018 camunda services GmbH (info@camunda.com)
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client;

namespace ClientExample
{
    internal class MainClass
    {
        private static readonly string DemoProcessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/demo-process.bpmn");

        public static async Task Main(string[] args)
        {
            // create zeebe client
            var client = ZeebeClient.NewZeebeClient("192.168.30.220:26500");

            // deploy
            var deployResponse = await client.NewDeployCommand().AddResourceFile(DemoProcessPath).Send();

            // create workflow instance
            var workflowKey = deployResponse.Workflows[0].WorkflowKey;
            await client
                .NewCreateWorkflowInstanceCommand()
                .WorkflowKey(workflowKey)
                .Payload("{\"foo\":\"123\"}")
                .Send();

            // open job worker
            using (var signal = new EventWaitHandle(false, EventResetMode.AutoReset))
            {
                client.NewWorker()
                      .JobType("foo")
                      .Handler((jobClient, job) =>
                      {
                          // business logic
                          var jobKey = job.Key;
                          Console.WriteLine("Handle job: " + jobKey);

                          if (jobKey % 2 == 0)
                          {
                              jobClient.NewCompleteJobCommand(jobKey).Payload("{\"foo\":2}").Send();
                          }
                          else
                          {
                              jobClient.NewFailCommand(jobKey).Retries(job.Retries - 1).ErrorMessage("Example fail").Send();
                          }
                      })
                      .Limit(5)
                      .Name("csharpWorker")
                      .PollInterval(TimeSpan.FromSeconds(1))
                      .Timeout(TimeSpan.FromSeconds(10))
                      .Open();

                // blocks main thread, so that worker can run
                signal.WaitOne();
            }
        }
    }
}
