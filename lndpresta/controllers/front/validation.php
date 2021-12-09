<?php
/*
* 2007-2015 PrestaShop
*
* NOTICE OF LICENSE
*
* This source file is subject to the Academic Free License (AFL 3.0)
* that is bundled with this package in the file LICENSE.txt.
* It is also available through the world-wide-web at this URL:
* http://opensource.org/licenses/afl-3.0.php
* If you did not receive a copy of the license and are unable to
* obtain it through the world-wide-web, please send an email
* to license@prestashop.com so we can send you a copy immediately.
*
* DISCLAIMER
*
* Do not edit or add to this file if you wish to upgrade PrestaShop to newer
* versions in the future. If you wish to customize PrestaShop for your
* needs please refer to http://www.prestashop.com for more information.
*
*  @author PrestaShop SA <contact@prestashop.com>
*  @copyright  2007-2015 PrestaShop SA
*  @license    http://opensource.org/licenses/afl-3.0.php  Academic Free License (AFL 3.0)
*  International Registered Trademark & Property of PrestaShop SA
*/

/**
 * @since 1.5.0
 */
class LndPrestaValidationModuleFrontController extends ModuleFrontController
{
    /**
     * @see FrontController::postProcess()
     */
    public function postProcess()
    {
        $cart = $this->context->cart;
        if ($cart->id_customer == 0 || $cart->id_address_delivery == 0 || $cart->id_address_invoice == 0 || !$this->module->active) {
            Tools::redirect('index.php?controller=order&step=1');
        }

        // Check that this payment option is still available in case the customer changed his address just before the end of the checkout process
        $authorized = false;
        foreach (Module::getPaymentModules() as $module) {
            if ($module['name'] == 'lndpresta') {
                $authorized = true;
                break;
            }
        }

        if (!$authorized) {
            die($this->module->l('This payment method is not available.', 'validation'));
        }

         $customer = new Customer($cart->id_customer);
        if (!Validate::isLoadedObject($customer))
             Tools::redirect('index.php?controller=order&step=1');

         $currency = $this->context->currency;
         $total = (float)$cart->getOrderTotal(true, Cart::BOTH);

         $queries = array();
         parse_str($_SERVER['QUERY_STRING'], $queries);
         
         if(isset($queries) && isset($queries['param']))
         {
            $param = $queries['param'];
            $decrypted = $this->decrypt($param,Configuration::get('lnd_invoice_SECRETKEY'));

            $shop_name = Configuration::get('PS_SHOP_NAME');

            $check = $shop_name . ':' . $shop_name . ' invoice:' . $this->context->cart->id .':' . $currency->iso_code.':' . number_format((float)$total, 2, '.', '') . 'PAID';

            if($decrypted == $check)
            {
                $this->module->validateOrder($cart->id, 2, $total, $this->module->displayName, NULL, NULL, (int)$currency->id, false, $customer->secure_key);
                Tools::redirect('index.php?controller=order-confirmation&id_cart='.$cart->id.'&id_module='.$this->module->id.'&id_order='.$this->module->currentOrder.'&key='.$customer->secure_key);   
            }
            else{
                Tools::redirect('index.php?controller=order&step=1');    
            }
        }
         else{
            Tools::redirect('index.php?controller=order&step=1');
         }
    }
    
    public function decrypt($string, $secret)
    {
        echo('nocrypt');
        //Generate a key from a hash
        $key = md5($secret, true);

        //Take first 8 bytes of $key and append them to the end of $key.
        $key .= substr($key, 0, 8);

        // openssl
        return openssl_decrypt(base64_decode($string),'DES-EDE3',$key,OPENSSL_RAW_DATA);
    }

    private function checkIfPaymentOptionIsAvailable()
    {
        $modules = Module::getPaymentModules();

        if (empty($modules)) {
            return false;
        }

        foreach ($modules as $module) {
            if (isset($module['name']) && $this->module->name === $module['name']) {
                return true;
            }
        }

        return false;
    }
}
