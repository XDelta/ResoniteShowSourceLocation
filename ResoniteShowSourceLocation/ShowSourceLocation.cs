using HarmonyLib;
using ResoniteModLoader;
using FrooxEngine;
using Elements.Core;
using FrooxEngine.UIX;

namespace ShowSourceLocation;

public class ResoniteShowSourceLocation : ResoniteMod {
	internal const string VERSION_CONSTANT = "1.2.0";
	public override string Name => "Show Source Location";
	public override string Author => "Delta";
	public override string Version => VERSION_CONSTANT;
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

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<Chirality> workerInspectorButtonSide = new ModConfigurationKey<Chirality>("workerInspectorButtonSide", "What side to show the open slot button on worker inspectors", () => Chirality.Left);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> altButtonColor = new ModConfigurationKey<bool>("altButtonColor", "Alternative button color, grey instead of purple", () => true);


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
			__instance.LocalUser.GetPointInFrontOfUser(out float3 pos, out _, distance: 0.35f);
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
		public static void Postfix(WorkerInspector __instance, bool allowContainer) {
			if (!Config.GetValue(enabled)) {
				return;
			}

			if (allowContainer && __instance.FindNearestParent<Slot>() != null) {
				var buttons = __instance.Slot.GetComponentsInChildren<Button>();
				var targetMethod = AccessTools.Method(__instance.GetType(), "OnOpenContainerPressed");

				foreach (var button in buttons) {
					var buttonRefRelay = button.Slot.GetComponent<ButtonRefRelay<Worker>>();
					if (buttonRefRelay != null && buttonRefRelay.ButtonPressed.Target.Method == targetMethod) {
						if (Config.GetValue(workerInspectorButtonSide) == Chirality.Left) {
							button.Slot.OrderOffset = -1L;
						}
						if (Config.GetValue(altButtonColor)) {
							button.Slot.GetComponentInChildren<Image>().Tint.Value = RadiantUI_Constants.BUTTON_COLOR;
						}
						break;
					}
				}
			}
		}
	}
}