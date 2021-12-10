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

if (!defined('_PS_VERSION_')) {
    exit;
}

class LndPresta extends PaymentModule
{
    protected $_html = '';
    protected $_postErrors = array();

    public $details;
    public $owner;
    public $address;
    public $extra_mail_vars;

    public function __construct()
    {
        $this->name = 'lndpresta';
        $this->tab = 'payments_gateways';
        $this->version = '1.0.0';
        $this->ps_versions_compliancy = array('min' => '1.7', 'max' => _PS_VERSION_);
        $this->author = 'PrestaShop modified by Asyncluck';
        $this->controllers = array('validation');
        $this->is_eu_compatible = 1;

        $this->currencies = true;
        $this->currencies_mode = 'checkbox';

        $this->bootstrap = true;
        parent::__construct();

        $this->displayName = $this->l('Lnd payment');
        $this->description = $this->l('Pay with LND node Blazor app');

        if (!count(Currency::checkPaymentCurrencies($this->id))) {
            $this->warning = $this->l('No currency has been set for this module.');
        }
    }

    public function install()
    {
        if (!parent::install() || !$this->registerHook('paymentOptions') || !$this->registerHook('paymentReturn')) {
            return false;
        }

        return true;
    }

    public function uninstall()
    {
        Configuration::deleteByName('lnd_invoice_URL');
        Configuration::deleteByName('lnd_invoice_FRONTENDURL');
        Configuration::deleteByName('lnd_invoice_EXPIRY');
        Configuration::deleteByName('lnd_invoice_SECRETKEY');

        return parent::uninstall();
    }

    public function getContent()
    {
        if (Tools::isSubmit('submitlnd')) {
            $this->_errors = array();
            if (Tools::getValue('lnd_invoice_url') == NULL) {
                $this->_errors[]  = $this->l('Missing url for api');
            }
            if (Tools::getValue('lnd_invoice_frontendurl') == NULL) {
                $this->_errors[]  = $this->l('Missing url for frontend payment module');
            }
            if (Tools::getValue('lnd_invoice_expiry') == NULL) {
                $this->_errors[]  = $this->l('Missing expiry in second for an invoice');
            }
            if (Tools::getValue('lnd_invoice_secretkey') == NULL) {
                $this->_errors[]  = $this->l('Missing secret key');
            }

            if (count($this->_errors) > 0) {
                $error_msg = '';
                foreach ($this->_errors AS $error) {
                    $error_msg .= $error.'<br />';
                }
                $this->_html = $this->displayError($error_msg);
            } else {
                Configuration::updateValue('lnd_invoice_URL', trim(Tools::getValue('lnd_invoice_url')));
                Configuration::updateValue('lnd_invoice_FRONTENDURL', trim(Tools::getValue('lnd_invoice_frontendurl')));
                Configuration::updateValue('lnd_invoice_EXPIRY', trim(Tools::getValue('lnd_invoice_expiry')));
                Configuration::updateValue('lnd_invoice_SECRETKEY', trim(Tools::getValue('lnd_invoice_secretkey')));
                $this->_html = $this->displayConfirmation($this->l('Settings updated'));
            }
        }

        $this->_html .= '
            <h5>'.$this->l('This module allows you to accept payments via Lightning.').'</h5><div style="clear:both;">&nbsp;</div>';


        $this->_html .= '<form method="post" action="'.htmlentities($_SERVER['REQUEST_URI']).'">
        <h3>Settings</h3>
        <p class="left">
            <label for="lnd_invoice_url" style="float:none">'.$this->l('Lnd invoice api url').'</label>
            <input type="text" class="form-control" id="lnd_invoice_url" name="lnd_invoice_url" 
                value="'.htmlentities(Tools::getValue('url', Configuration::get('lnd_invoice_URL')), ENT_COMPAT, 'UTF-8').'"
             />
        </p>
        <p class="left">
            <label for="lnd_invoice_frontendurl" style="float:none">'.$this->l('Lnd invoice frontend url').'</label>
            <input type="text" class="form-control" id="lnd_invoice_frontendurl" name="lnd_invoice_frontendurl" 
                value="'.htmlentities(Tools::getValue('fronturl', Configuration::get('lnd_invoice_FRONTENDURL')), ENT_COMPAT, 'UTF-8').'"
             />
        </p>
        <p class="left">
            <label for="lnd_invoice_expiry" style="float:none">'.$this->l('Lnd invoice expiry for invoice in seconds').'</label>
            <input type="text" class="form-control" id="lnd_invoice_expiry" name="lnd_invoice_expiry" 
                value="'.htmlentities(Tools::getValue('expiry', Configuration::get('lnd_invoice_EXPIRY')), ENT_COMPAT, 'UTF-8').'"
             />
        </p>
        <p class="left">
        <label for="lnd_invoice_secretkey" style="float:none">'.$this->l('Lnd front end secret key').'</label>
        <input type="text" class="form-control" id="lnd_invoice_secretkey" name="lnd_invoice_secretkey" 
            value="'.htmlentities(Tools::getValue('expiry', Configuration::get('lnd_invoice_SECRETKEY')), ENT_COMPAT, 'UTF-8').'"
         />
    </p>
        <p class="center"><input class="button" type="submit" name="submitlnd" value="'.$this->l('Save settings').'" /></p>
        </form>'
        ;

        return $this->_html;
    }

    public function hookPaymentOptions($params)
    {
        if (!$this->active) {
            return;
        }

        if (!$this->checkCurrency($params['cart'])) {
            return;
        }

        $payment_options = [
              $this->getExternalPaymentOption(),
        ];

        return $payment_options;
    }

    public function checkCurrency($cart)
    {
        $currency_order = new Currency($cart->id_currency);
        $currencies_module = $this->getCurrency($cart->id_currency);

        if (is_array($currencies_module)) {
            foreach ($currencies_module as $currency_module) {
                if ($currency_order->id == $currency_module['id_currency']) {
                    return true;
                }
            }
        }
        return false;
    }

    public function getExternalPaymentOption()
    {
        $externalOption = new \PrestaShop\PrestaShop\Core\Payment\PaymentOption();
        $externalOption->setCallToActionText($this->l('Pay with Lightning Network'))
                       ->setAction($this->context->link->getModuleLink($this->name, 'payment', array(), true))
                       ->setLogo(Media::getMediaPath(_PS_MODULE_DIR_.$this->name.'/payment.jpg'));
                       //  ->setAdditionalInformation($this->context->smarty->fetch('module:lndpresta/views/templates/front/payment_infos.tpl'))
                        
        return $externalOption;
    }


    //*************************************** */
    // Create invoice
    //*************************************** */
    public function generatePayment($cart)
    {
        
        $currency = Currency::getCurrencyInstance((int)$cart->id_currency);
        $options = $_POST;
        $currency_iso = $currency->iso_code;
        $total = $cart->getOrderTotal(true);

        $this->key = $this->context->customer->secure_key;
    
        //Debug
        //     var_dump(Configuration::get('lnd_invoice_URL'));
        //     var_dump(Configuration::get('lnd_invoice_EXPIRY'));
        //     var_dump(Configuration::get('lnd_invoice_FRONTENDURL'));
        //    var_dump($cart->id);

        //Create invoice via API
        $shop_name = Configuration::get('PS_SHOP_NAME');
        
        $curl = curl_init(Configuration::get('lnd_invoice_URL') . '/invoices');
        $data = array(
            'shopName' => $shop_name,
            'currency' => $currency_iso,
            'amount' => $total,
            'description' => $shop_name . ' invoice',
            'invoiceIdentifier' => strval($cart->id),
            'expiryInSec' => (int)Configuration::get('lnd_invoice_EXPIRY')
        );
       
        $payload = json_encode($data);
        curl_setopt($curl, CURLOPT_POSTFIELDS, $payload);
        curl_setopt($curl, CURLOPT_HTTPHEADER, array('Content-Type:application/json'));
        curl_setopt($curl, CURLOPT_RETURNTRANSFER, true);
        curl_setopt($curl, CURLOPT_SSL_VERIFYHOST, false);
        curl_setopt($curl, CURLOPT_SSL_VERIFYPEER, false);

        $result = curl_exec($curl);

        if ( ! $result) {
            $result = curl_error($curl);
            die(Tools::displayError("Error: No data returned from API server. "));
        } 

        //DEBUG
        // var_dump($result);
        
       $response = json_decode($result, true);
       
        curl_close($curl);
       
        if (empty($response)) {
             die(Tools::displayError("Empty response received "));
        }

        if (empty($response['r_hash_str'])) {
            die(Tools::displayError("Empty r_hash "));
       }

       
       $encryptedR_hash = urlencode($this->encrypt($response['r_hash_str'],Configuration::get('lnd_invoice_SECRETKEY')));

       $redirectUrl = $this->context->link->getModuleLink($this->name, 'validation', array(), true);
       $encryptedRedirectUrl = urlencode($this->encrypt($redirectUrl,Configuration::get('lnd_invoice_SECRETKEY')));

       Tools::redirect(Configuration::get('lnd_invoice_FRONTENDURL') . '/?r_hash_str=' . $encryptedR_hash . '&redirect=' . $encryptedRedirectUrl);

        //DEBUG
        // foreach ($response as $key => $value) {
        //     echo "$key | $value <br/>";
        
        // }

        // echo 'test: ' . $response['r_hash_str'];

    }

    public function encrypt($string, $secret)
    {
       //Generate a key from a hash
        $key = md5($secret, true);

        //Take first 8 bytes of $key and append them to the end of $key.
        $key .= substr($key, 0, 8);

        // openssl
        return base64_encode(openssl_encrypt( $string, 'DES-EDE3', $key,  OPENSSL_RAW_DATA));
    }


}
