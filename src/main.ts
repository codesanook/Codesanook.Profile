import './sass/style.scss';

// Export all React components inside a component folder
import * as Components from './components';
declare var global: any;

// Export as a module name to not be overried by other modules
global.CodesanookBasicUserProfile = Components;
