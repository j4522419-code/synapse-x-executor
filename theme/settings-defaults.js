function synapseSettingsUpgrade(prefs,storage){
  Object.assign(prefs,{purchases:'allow',theme:'synapse-x',customBase:'synapse-x',customColors:{},customCss:'',savedThemes:[],mcpEnabled:false});
  if(storage.getItem('synapse-x.settings.v1.3.2')!=='1'){
    Object.assign(prefs,{
      insertSpaces:true,wordWrap:true,minimap:true,lineNumbers:true,bracketColors:true,smoothCursor:true,smoothScrolling:true,
      folding:true,autocomplete:true,diagnostics:true,multiInstance:true,
      showConsole:true,consoleTimestamps:true,redirectOutput:true,redirectErrors:true,clearOnTeleport:true
    });
    storage.setItem('synapse-x.settings.v1.3.2','1');
  }
  return prefs;
}
