using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RomTools.Plugins
{

  public static class PluginManager
  {

    #region Data Members

    private static Dictionary<string, Plugin> _loadedPlugins;

    #endregion

    #region Constructor

    static PluginManager()
    {
      _loadedPlugins = new Dictionary<string, Plugin>();
    }

    #endregion

    #region Public Methods

    public static Result LoadPlugin( Type pluginDefinitionType )
    {
      if ( _loadedPlugins.ContainsKey( pluginDefinitionType.FullName ) )
        return Result.Failure( $"Plugin already loaded: {pluginDefinitionType.Name}" );

      try
      {
        var pluginInstance = (Plugin)Activator.CreateInstance( pluginDefinitionType );

        pluginInstance.Initialize();
        _loadedPlugins.Add( pluginDefinitionType.FullName, pluginInstance );

        return Result.Successful( $"Loaded plugin {pluginInstance.PluginName} ({pluginInstance.PluginVersion})" );
      }
      catch( Exception ex )
      {
        return Result.Failure( $"Failed to load plugin `{pluginDefinitionType.Name}`: {ex.Message}" );
      }
    }

    public static Result LoadPluginFromAssembly( Assembly pluginAssembly )
    {
      // Get plugin definitions
      var pluginDefinitions = pluginAssembly.GetTypes()
        .Where( x => !x.IsAbstract && typeof( Plugin ).IsAssignableFrom( x ) )
        .ToArray();

      if ( pluginDefinitions.Length == 0 )
        return Result.Failure( $"The provided assembly does not contain a plugin: `{pluginAssembly.FullName}`" );
      if ( pluginDefinitions.Length > 1 )
        return Result.Failure( $"The provided assembly contains more than one plugin definition: `{pluginAssembly.FullName}`" );

      return LoadPlugin( pluginDefinitions.First() );
    }

    #endregion

  }

}