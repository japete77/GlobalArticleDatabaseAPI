Customizations to enable WordPress integration through Web-Api

1. Install plugín Swagger API for WordPress
		
2. Install plugin: JWT Authentication for WP REST API and follows instructions to configure .htaccess and wp-config.php

	Reference: https://www.youtube.com/watch?v=Mp7T7x1oxDk

3. Enable retrieval of all users regardless of have published posts
	
	Go to menu Theme > Edit themes > Edit functions.php and add the following web hook
	
		add_filter( 'rest_user_query' , 'custom_rest_user_query' );
		
		function custom_rest_user_query( $prepared_args, $request = null ) {
		  unset($prepared_args['has_published_posts']);
		  return $prepared_args;
		}

https://latidocreativodigital.es/pxevangelio/wp-content/plugins/wp-config-file-editor/Public/Restore.php?absPath=%2Fvar%2Fwww%2Fvhosts%2Flatidocreativodigital.es%2Fpublic_html%2Fpxevangelio&contentDir=%2Fvar%2Fwww%2Fvhosts%2Flatidocreativodigital.es%2Fpublic_html%2Fpxevangelio%2Fwp-content%2Fwcfe-9be3f76294085aa6fde7f5917cf7e286&secureKey=2bacfba943d7f47ee4af5ca3b9ae3bd8&dataFileSecure=5f84de6286cc120b25b825ccb09deaf9