using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Safe.Exceptions;
using kOS.Safe.Utilities;
using kOS.Suffixed;
using kOS.Utilities;
using kOS.AddOns;
using kOS;

namespace kOMAS
{
    [kOSAddon("KOMAS")]
    [KOSNomenclature("KOMASAddon")]
    public class kOMASAPI : kOS.Suffixed.Addon
    {
        private kOMASButtonAPI buttons;
        private kOMASFlagAPI flags;

        public kMASAPI(SharedObjects shared) : base(shared)
        {
            AddSuffix("BUTTONS", new Suffix<kOMASButtonAPI>(GetButtons));
            AddSuffix("FLAGS", new Suffix<kOMASFlagAPI>(GetFlags));
            AddSuffix("GETGUID", new OneArgsSuffix<StringValue, ScalarIntValue>((ScalarIntValue index) => GetGUID(index, false)));
            AddSuffix("GETGUIDSHORT", new OneArgsSuffix<StringValue, ScalarIntValue>((ScalarIntValue index) => GetGUID(index, true)));
            AddSuffix("GETMONITORCOUNT", new NoArgsSuffix<ScalarIntValue>(GetMonitorCount));
            AddSuffix("GETINDEXOF", new OneArgsSuffix<ScalarIntValue, StringValue>(IndexOf));
        }

        public override BooleanValue Available()
        {
            return kOMASCore.fetch.vessel_register.ContainsKey(shared.Vessel.id);
        }

        private kOMASButtonAPI GetButtons()
        {
            if (buttons == null)
            {
                buttons = new kOMASButtonAPI(shared);
            }
            return buttons;
        }

        private kOMASFlagAPI GetFlags()
        {
            if (flags == null)
            {
                flags = new kOMASFlagAPI(shared);
            }
            return flags;
        }

        private StringValue GetGUID(ScalarIntValue index, bool shortGuid)
        {
            if (!kOMASCore.fetch.vessel_register.ContainsKey(shared.Vessel.id)) return "";
            if (kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors.Count <= index) throw new KOSException("Cannot get monitor guid, input out of range.");

            if (shortGuid) return kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors[index].ToString().Substring(0, 8);
            return kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors[index].ToString();
        }

        private ScalarIntValue GetMonitorCount()
        {
            if (!kOMASCore.fetch.vessel_register.ContainsKey(shared.Vessel.id)) return 0;
            return kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors.Count;
        }

        private ScalarIntValue IndexOf(StringValue guid)
        {
            if (!kOMASCore.fetch.vessel_register.ContainsKey(shared.Vessel.id)) return -1;
            foreach (KeyValuePair<int, Guid> kvpair in kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors)
            {
                if (guid == kvpair.Value.ToString().Substring(0, 8)) return kvpair.Key;
                if (guid == kvpair.Value.ToString()) return kvpair.Key;
            }
            return -1;
        }
    }

    [KOSNomenclature("BUTTONS")]
    public class kOMASButtonAPI : Structure
    {
        private SharedObjects shared;
        private int monitor = 0;

        public kOMASButtonAPI(SharedObjects shared)
        {
            this.shared = shared;
            AddSuffix("CURRENTMONITOR", new SetSuffix<ScalarIntValue>(() => { return monitor; }, (monitor) => { this.monitor = monitor; }));
            AddSuffix("GETLABEL", new OneArgsSuffix<StringValue, ScalarIntValue>(GetButtonLabel));
            AddSuffix("SETLABEL", new TwoArgsSuffix<ScalarIntValue, StringValue>(SetButtonLabel));
            AddSuffix("GETSTATE", new OneArgsSuffix<BooleanValue, ScalarIntValue>(GetButtonState));
            AddSuffix("SETSTATE", new TwoArgsSuffix<ScalarIntValue, BooleanValue>(SetButtonState));
        }

        private StringValue GetButtonLabel(ScalarIntValue value)
        {
            if (!kOMASCore.fetch.vessel_register.ContainsKey(shared.Vessel.id)) return "";
            if (kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors.Count == 0 || kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors.Count <= monitor) return "";
            if (!kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).buttonLabels[monitor].ContainsKey(value)) throw new KOSException("Cannot get button status, input out of range.");

            return kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).buttonLabels[monitor][value];
        }

        private void SetButtonLabel(ScalarIntValue index, StringValue value)
        {
            if (!kOMASCore.fetch.vessel_register.ContainsKey(shared.Vessel.id)) return;
            if (kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors.Count == 0 || kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors.Count <= monitor) return;
            kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).buttonLabels[monitor][index] = value;
        }

        private BooleanValue GetButtonState(ScalarIntValue value)
        {
            if (!kOMASCore.fetch.vessel_register.ContainsKey(shared.Vessel.id)) return false;
            if (kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors.Count == 0 || kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors.Count <= monitor) return false;
            if (value < 0)
            {
                if (value == -1) return kOMASCore.fetch.monitor_register[kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors[monitor]].enterButtonState;
                if (value == -2) return kOMASCore.fetch.monitor_register[kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors[monitor]].cancelButtonState;
                if (value == -3) return kOMASCore.fetch.monitor_register[kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors[monitor]].upButtonState;
                if (value == -4) return kOMASCore.fetch.monitor_register[kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors[monitor]].downButtonState;
                if (value == -5) return kOMASCore.fetch.monitor_register[kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors[monitor]].leftButtonState;
                if (value == -6) return kOMASCore.fetch.monitor_register[kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors[monitor]].rightButtonState;
            }
            if (!kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).buttonStates[monitor].ContainsKey(value)) throw new KOSException("Cannot get button status, input out of range.");
            return kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).buttonStates[monitor][value];
        }

        private void SetButtonState(ScalarIntValue index, BooleanValue value)
        {
            if (!kOMASCore.fetch.vessel_register.ContainsKey(shared.Vessel.id)) return;
            if (kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors.Count == 0 || kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors.Count <= monitor) return;
            if (index < 0)
            {
                if (index == -1) kOMASCore.fetch.monitor_register[kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors[monitor]].enterButtonState = value;
                if (index == -2) kOMASCore.fetch.monitor_register[kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors[monitor]].cancelButtonState = value;
                if (index == -3) kOMASCore.fetch.monitor_register[kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors[monitor]].upButtonState = value;
                if (index == -4) kOMASCore.fetch.monitor_register[kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors[monitor]].downButtonState = value;
                if (index == -5) kOMASCore.fetch.monitor_register[kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors[monitor]].leftButtonState = value;
                if (index == -6) kOMASCore.fetch.monitor_register[kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors[monitor]].rightButtonState = value;
                return;
            }
            kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).buttonStates[monitor][index] = value;
        }
    }

    [KOSNomenclature("FLAGS")]
    public class kOMASFlagAPI : Structure
    {
        private SharedObjects shared;
        private int monitor = 0;

        public kOMASFlagAPI(SharedObjects shared)
        {
            this.shared = shared;
            AddSuffix("CURRENTMONITOR", new SetSuffix<ScalarIntValue>(() => { return monitor; }, (monitor) => { this.monitor = monitor; }));
            AddSuffix("GETLABEL", new OneArgsSuffix<StringValue, ScalarIntValue>(GetFlagLabel));
            AddSuffix("SETLABEL", new TwoArgsSuffix<ScalarIntValue, StringValue>(SetFlagLabel));
            AddSuffix("GETSTATE", new OneArgsSuffix<BooleanValue, ScalarIntValue>(GetFlagState));
            AddSuffix("SETSTATE", new TwoArgsSuffix<ScalarIntValue, BooleanValue>(SetFlagState));
        }

        private StringValue GetFlagLabel(ScalarIntValue value)
        {
            if (!kOMASCore.fetch.vessel_register.ContainsKey(shared.Vessel.id)) return "";
            if (kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors.Count == 0 || kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors.Count <= monitor) return "";
            if (!kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).flagLabels[monitor].ContainsKey(value)) throw new KOSException("Cannot get button status, input out of range.");

            return kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).flagLabels[monitor][value];
        }

        private void SetFlagLabel(ScalarIntValue index, StringValue value)
        {
            if (!kOMASCore.fetch.vessel_register.ContainsKey(shared.Vessel.id)) return;
            if (kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors.Count == 0 || kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors.Count <= monitor) return;
            kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).flagLabels[monitor][index] = value;
        }

        private BooleanValue GetFlagState(ScalarIntValue value)
        {
            if (!kOMASCore.fetch.vessel_register.ContainsKey(shared.Vessel.id)) return false;
            if (kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors.Count == 0 || kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors.Count <= monitor) return false;
            if (!kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).flagStates[monitor].ContainsKey(value)) throw new KOSException("Cannot get button status, input out of range.");

            return kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).flagStates[monitor][value];
        }

        private void SetFlagState(ScalarIntValue index, BooleanValue value)
        {
            if (!kOMASCore.fetch.vessel_register.ContainsKey(shared.Vessel.id)) return;
            if (kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors.Count == 0 || kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).monitors.Count <= monitor) return;
            kOMASCore.fetch.GetVesselMonitors(shared.Vessel.id).flagStates[monitor][index] = value;
        }
    }
}