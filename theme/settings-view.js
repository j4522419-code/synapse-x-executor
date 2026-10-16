UZe=Bn({__name:"settings_view",props:{username:{}},emits:["logout"],setup(props,{emit}){
  const current=ft('appearance'),query=ft('');
  const pages=[
    {id:'appearance',label:'Appearance',glyph:'palette',description:'Control what is shown while you work or share your screen.',rows:[
      {key:'streamerMode',label:'Streamer mode',description:'Blur private details in the interface while sharing your screen.'},
      {key:'tooltips',label:'Tooltips',description:'Show short labels when you hover over buttons and icons.'}
    ]},
    {id:'editor',label:'Editor',glyph:'code',description:'Set up indentation, code visibility and movement in the editor.',rows:[
      {key:'insertSpaces',label:'Insert spaces',description:'Use spaces instead of tab characters when you indent a line.'},
      {key:'wordWrap',label:'Word wrap',description:'Continue long lines on the next visible line without changing the script.'},
      {key:'minimap',label:'Minimap',description:'Show a small overview of the script along the edge of the editor.'},
      {key:'lineNumbers',label:'Line numbers',description:'Display a line number beside each line of code.'},
      {key:'bracketColors',label:'Bracket colors',description:'Color matching brackets so nested code is easier to follow.'},
      {key:'smoothCursor',label:'Smooth cursor',description:'Animate the caret as it moves between positions.'},
      {key:'smoothScrolling',label:'Smooth scrolling',description:'Move smoothly through the script when you scroll.'}
    ]},
    {id:'game',label:'Game',glyph:'globe',description:'Manage the runtime features available in game.',rows:[
      {key:'rakNet',label:'Enable RakNet',description:'Make the runtime’s RakNet functions available to scripts.'}
    ]},
    {id:'signout',label:'Sign out',glyph:'signout',description:'End your current session and return to the sign-in screen.',rows:[]}
  ];
  function icon(name){
    if(name!=='signout')return ye(Ve,{name,size:16});
    const paths=['M9 3H4v18h5','M12 12h9','m17 8 4 4-4 4'];
    return E('svg',{width:16,height:16,viewBox:'0 0 24 24',fill:'none',stroke:'currentColor','stroke-width':1.7,'stroke-linecap':'round','stroke-linejoin':'round','aria-hidden':'true'},paths.map(d=>E('path',{d})));
  }
  function toggle(row){return E('label',{class:'row',key:row.key},[
    E('div',{class:'label'},[E('span',null,row.label),E('small',null,row.description)]),
    E('input',{type:'checkbox',class:'switch',checked:X[row.key],'aria-label':row.label,onChange:event=>{X[row.key]=event.target.checked;}})
  ]);}
  return()=>{
    const needle=query.value.trim().toLowerCase(),matches=text=>text.toLowerCase().includes(needle);
    const shown=needle?pages.map(page=>({...page,rows:matches(page.label)?page.rows:page.rows.filter(row=>matches(row.label+' '+row.description))})).filter(page=>page.rows.length||(page.id==='signout'&&matches(page.label+' '+page.description))):[pages.find(page=>page.id===current.value)];
    return E('div',{class:'settings sx-settings'},[
      E('nav',{class:'rail','aria-label':'Settings categories'},[
        E('div',{class:'rail-search'},[icon('search'),E('input',{type:'search',placeholder:'Find a setting',spellcheck:false,'aria-label':'Search settings',value:query.value,onInput:event=>{query.value=event.target.value;}})]),
        ...pages.map(page=>E('button',{type:'button',class:Ft(['rail-row',{'is-active':!needle&&current.value===page.id,'sx-signout-rail':page.id==='signout'}]),'aria-current':!needle&&current.value===page.id?'page':null,onClick:()=>{current.value=page.id;query.value='';}},[icon(page.glyph),E('span',null,page.label)]))
      ]),
      E('div',{class:'pane'},[E('div',{class:'pane-scroll'},[E('div',{class:'inner'},[
        ...shown.map(page=>E('section',{key:page.id,'data-settings-page':page.id},[
          E('h2',null,page.label),E('p',{class:'sx-settings-intro'},page.description),
          page.id==='signout'?E('div',{class:'card sx-signout-card'},[
            icon('signout'),E('p',null,'You will need to sign in again to use Synapse X Executor.'),
            E('button',{type:'button',class:'chip-button sx-signout-action',onClick:()=>emit('logout')},'Sign out')
          ]):E('div',{class:'card'},page.rows.map(toggle))
        ])),
        shown.length?null:E('p',{class:'no-results',role:'status'},'No settings found. Try a different word.')
      ])])])
    ]);
  };
}})
