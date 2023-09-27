using HarmonyLib;
using ResoniteModLoader;
using FrooxEngine;
using Elements.Core;
using FrooxEngine.UIX;

namespace ShowSourceLocation {
	public class ResoniteShowSourceLocation : ResoniteMod {
		public override string Name => "Show Source Location";
		public override string Author => "Delta";
		public override string Version => "1.1.1";
		public override string Link => "https://github.com/XDelta/ResoniteShowSourceLocation";

		private enum ViewOptions {
			ShowTextInWorld,
            ShowTextInUserspace,
            Hide
        }
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> enabled = new ModConfigurationKey<bool>("enabled", "Should the mod be enabled", () => true);

        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<ViewOptions> showTextInWorld = new ModConfigurationKey<ViewOptions>("showTextInWorld", "Show floating text in world", () => ViewOptions.ShowTextInWorld);

        private static ModConfiguration Config;
		public override void OnEngineInit() {
			Config = GetConfiguration();
			Config.Save(true);
			Harmony harmony = new("net.deltawolf.ShowSourceLocation");
			harmony.PatchAll();
		}

		[HarmonyPatch(typeof(Button), "RunPressed")]
		class Button_RunPressed_Patch {
			public static void Postfix(Button __instance) {
				if (!Config.GetValue(enabled)) {
					return;
				}
				ReferenceProxySource refProxy = __instance.Slot.GetComponent<ReferenceProxySource>();
				if (refProxy == null) {
					return;
				}
                if (refProxy.Reference.Target is not IField field || (!field.IsDriven && !field.IsLinked)) {
                    return;
                }
                SyncElement syncElement = field.ActiveLink as SyncElement;
				InspectorHelper.OpenInspectorForTarget(syncElement.Component, null, true);
				__instance.LocalUser.GetPointInFrontOfUser(out float3 pos, out floatQ rot, distance: 0.35f);
				switch (Config.GetValue(showTextInWorld)) {
					case ViewOptions.ShowTextInWorld:
                        NotificationMessage.SpawnTextMessage(__instance.World, pos, syncElement.Name, colorX.Magenta);
                        break;
                    case ViewOptions.ShowTextInUserspace:
                        NotificationMessage.SpawnTextMessage(syncElement.Name, colorX.Magenta);
						break;
                    default:
						break;
				}
			}
		}

		[HarmonyPatch(typeof(WorkerInspector), "BuildUIForComponent")]
		class WorkerInspector_BuildUIForComponent_Patch {
			public static void Postfix(bool allowRemove, bool allowDuplicate, WorkerInspector __instance) {
                if (!Config.GetValue(enabled)) {
                    return;
                }
                if (allowRemove && !allowDuplicate) {
					UIBuilder ui = new(__instance.Slot[0][0]);
					RadiantUI_Constants.SetupEditorStyle(ui, false);
					ui.Style.MinHeight = 24f;
					ui.Style.FlexibleWidth = 0f;
					ui.Style.MinWidth = 40f;

					Button button = ui.Button("⤴", RadiantUI_Constants.BUTTON_COLOR);
					button.Slot.OrderOffset = -1L;
					var refEditor = button.Slot[0].AttachComponent<RefEditor>();
					(AccessTools.Field(refEditor.GetType(), "_targetRef").GetValue(refEditor) as RelayRef<ISyncRef>).Target = (ISyncRef)AccessTools.Field(__instance.GetType(), "_targetWorker").GetValue(__instance);
					button.Pressed.Target = (ButtonEventHandler)AccessTools.Method(refEditor.GetType(), "OpenInspectorButton").CreateDelegate(typeof(ButtonEventHandler), refEditor);
				}
			}
		}
	}
}